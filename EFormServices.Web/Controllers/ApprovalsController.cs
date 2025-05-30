// EFormServices.Web/Controllers/ApprovalsController.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Domain.Entities;
using EFormServices.Domain.Enums;
using EFormServices.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/approvals")]
[Authorize]
public class ApprovalsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ApprovalsController> _logger;

    public ApprovalsController(
        ApplicationDbContext context,
        ICurrentUserService currentUser,
        ILogger<ApprovalsController> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _logger = logger;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingApprovals([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Unauthorized();

        if (!_currentUser.HasPermission("approve_forms"))
            return Forbid("Insufficient permissions");

        var query = _context.ApprovalProcesses
            .Where(ap => ap.Status == ApprovalStatus.InProgress)
            .Include(ap => ap.FormSubmission)
                .ThenInclude(fs => fs.Form)
            .Include(ap => ap.FormSubmission)
                .ThenInclude(fs => fs.SubmittedByUser)
            .Include(ap => ap.CurrentStep)
            .Where(ap => ap.FormSubmission.Form.OrganizationId == _currentUser.OrganizationId)
            .OrderBy(ap => ap.StartedAt);

        var totalCount = await query.CountAsync();
        var approvals = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ap => new
            {
                ProcessId = ap.Id,
                FormTitle = ap.FormSubmission.Form.Title,
                SubmissionId = ap.FormSubmissionId,
                TrackingNumber = ap.FormSubmission.TrackingNumber,
                SubmittedBy = ap.FormSubmission.SubmittedByUser.FullName,
                SubmittedAt = ap.FormSubmission.SubmittedAt,
                CurrentStep = ap.CurrentStep!.StepName,
                DaysWaiting = (DateTime.UtcNow - ap.StartedAt).Days
            })
            .ToListAsync();

        return Ok(new
        {
            items = approvals,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpPost("{processId}/approve")]
    public async Task<IActionResult> ApproveSubmission(int processId, [FromBody] ApprovalActionRequest request)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Unauthorized();

        if (!_currentUser.HasPermission("approve_forms"))
            return Forbid("Insufficient permissions");

        var process = await _context.ApprovalProcesses
            .Include(ap => ap.ApprovalWorkflow)
                .ThenInclude(aw => aw.ApprovalSteps)
            .Include(ap => ap.FormSubmission)
            .Include(ap => ap.CurrentStep)
            .FirstOrDefaultAsync(ap => ap.Id == processId);

        if (process == null)
            return NotFound("Approval process not found");

        if (process.Status != ApprovalStatus.InProgress)
            return BadRequest("Approval process is not in progress");

        process.AddAction(
            process.CurrentStepId!.Value,
            _currentUser.UserId.Value,
            ApprovalActionType.Approve,
            request.Comments
        );

        var nextStep = process.ApprovalWorkflow.GetNextStep(process.CurrentStep!.StepOrder);
        if (nextStep != null)
        {
            process.MoveToNextStep(nextStep.Id);
        }
        else
        {
            process.CompleteProcess();
            process.FormSubmission.Approve();
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Submission approved successfully" });
    }

    [HttpPost("{processId}/reject")]
    public async Task<IActionResult> RejectSubmission(int processId, [FromBody] ApprovalActionRequest request)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Unauthorized();

        if (!_currentUser.HasPermission("approve_forms"))
            return Forbid("Insufficient permissions");

        var process = await _context.ApprovalProcesses
            .Include(ap => ap.FormSubmission)
            .Include(ap => ap.CurrentStep)
            .FirstOrDefaultAsync(ap => ap.Id == processId);

        if (process == null)
            return NotFound("Approval process not found");

        if (process.Status != ApprovalStatus.InProgress)
            return BadRequest("Approval process is not in progress");

        if (string.IsNullOrWhiteSpace(request.Comments))
            return BadRequest("Comments are required for rejection");

        process.AddAction(
            process.CurrentStepId!.Value,
            _currentUser.UserId.Value,
            ApprovalActionType.Reject,
            request.Comments
        );

        process.RejectProcess(request.Comments);
        process.FormSubmission.Reject();

        await _context.SaveChangesAsync();

        return Ok(new { message = "Submission rejected successfully" });
    }

    [HttpGet("{processId}/history")]
    public async Task<IActionResult> GetApprovalHistory(int processId)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Unauthorized();

        var process = await _context.ApprovalProcesses
            .Include(ap => ap.FormSubmission)
                .ThenInclude(fs => fs.Form)
            .Include(ap => ap.ApprovalActions)
                .ThenInclude(aa => aa.ActionByUser)
            .Include(ap => ap.ApprovalActions)
                .ThenInclude(aa => aa.ApprovalStep)
            .FirstOrDefaultAsync(ap => ap.Id == processId);

        if (process == null)
            return NotFound("Approval process not found");

        if (process.FormSubmission.Form.OrganizationId != _currentUser.OrganizationId)
            return Forbid();

        var history = process.ApprovalActions
            .OrderBy(aa => aa.ActionAt)
            .Select(aa => new
            {
                aa.Action,
                aa.Comments,
                aa.ActionAt,
                ActionBy = aa.ActionByUser.FullName,
                StepName = aa.ApprovalStep.StepName
            })
            .ToList();

        return Ok(history);
    }
}

public class ApprovalActionRequest
{
    public string? Comments { get; set; }
}