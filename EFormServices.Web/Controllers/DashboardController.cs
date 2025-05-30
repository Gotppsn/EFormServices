// EFormServices.Web/Controllers/DashboardController.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Domain.Enums;
using EFormServices.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Unauthorized();

        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);

        var totalForms = await _context.Forms
            .CountAsync(f => f.OrganizationId == _currentUser.OrganizationId);

        var activeForms = await _context.Forms
            .CountAsync(f => f.OrganizationId == _currentUser.OrganizationId && f.IsActive && f.IsPublished);

        var totalSubmissions = await _context.FormSubmissions
            .Include(fs => fs.Form)
            .CountAsync(fs => fs.Form.OrganizationId == _currentUser.OrganizationId);

        var recentSubmissions = await _context.FormSubmissions
            .Include(fs => fs.Form)
            .CountAsync(fs => fs.Form.OrganizationId == _currentUser.OrganizationId && fs.SubmittedAt >= thirtyDaysAgo);

        var pendingApprovals = await _context.ApprovalProcesses
            .Include(ap => ap.FormSubmission)
                .ThenInclude(fs => fs.Form)
            .CountAsync(ap => ap.FormSubmission.Form.OrganizationId == _currentUser.OrganizationId && 
                            ap.Status == ApprovalStatus.InProgress);

        var topForms = await _context.Forms
            .Where(f => f.OrganizationId == _currentUser.OrganizationId && f.IsActive)
            .Select(f => new
            {
                f.Id,
                f.Title,
                SubmissionCount = f.FormSubmissions.Count(),
                RecentSubmissions = f.FormSubmissions.Count(fs => fs.SubmittedAt >= thirtyDaysAgo)
            })
            .OrderByDescending(f => f.SubmissionCount)
            .Take(5)
            .ToListAsync();

        var submissionTrend = await _context.FormSubmissions
            .Include(fs => fs.Form)
            .Where(fs => fs.Form.OrganizationId == _currentUser.OrganizationId && fs.SubmittedAt >= thirtyDaysAgo)
            .GroupBy(fs => fs.SubmittedAt.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count()
            })
            .OrderBy(g => g.Date)
            .ToListAsync();

        return Ok(new
        {
            overview = new
            {
                totalForms,
                activeForms,
                totalSubmissions,
                recentSubmissions,
                pendingApprovals
            },
            topForms,
            submissionTrend
        });
    }

    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetRecentActivity([FromQuery] int limit = 10)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Unauthorized();

        var recentSubmissions = await _context.FormSubmissions
            .Include(fs => fs.Form)
            .Include(fs => fs.SubmittedByUser)
            .Where(fs => fs.Form.OrganizationId == _currentUser.OrganizationId)
            .OrderByDescending(fs => fs.SubmittedAt)
            .Take(limit)
            .Select(fs => new
            {
                type = "submission",
                formTitle = fs.Form.Title,
                submittedBy = fs.SubmittedByUser.FullName,
                submittedAt = fs.SubmittedAt,
                trackingNumber = fs.TrackingNumber,
                status = fs.Status.ToString()
            })
            .ToListAsync();

        return Ok(recentSubmissions);
    }
}