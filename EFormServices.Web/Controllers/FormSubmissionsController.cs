// EFormServices.Web/Controllers/FormSubmissionsController.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Domain.Entities;
using EFormServices.Domain.Enums;
using EFormServices.Infrastructure.Data;
using EFormServices.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/forms/{formId}/submissions")]
public class FormSubmissionsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<FormSubmissionsController> _logger;

    public FormSubmissionsController(
        ApplicationDbContext context,
        ICurrentUserService currentUser,
        IFileStorageService fileStorage,
        ILogger<FormSubmissionsController> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> SubmitForm(int formId, [FromForm] FormSubmissionRequest request)
    {
        var form = await _context.Forms
            .Include(f => f.FormFields)
                .ThenInclude(ff => ff.Options)
            .Include(f => f.Organization)
            .FirstOrDefaultAsync(f => f.Id == formId);

        if (form == null)
            return NotFound("Form not found");

        if (!form.IsActive)
            return BadRequest("Form is not active");

        if (form.Settings.RequireAuthentication && !_currentUser.IsAuthenticated)
            return Unauthorized("Authentication required");

        if (!form.Settings.IsSubmissionAllowed(DateTime.UtcNow))
            return BadRequest("Form submission is not allowed at this time");

        var userId = _currentUser.UserId ?? 0;
        if (!_currentUser.IsAuthenticated)
        {
            var guestUser = await GetOrCreateGuestUser(form.OrganizationId);
            userId = guestUser.Id;
        }

        var submission = new FormSubmission(
            formId,
            userId,
            Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString()
        );

        _context.FormSubmissions.Add(submission);
        await _context.SaveChangesAsync();

        foreach (var fieldValue in request.FieldValues)
        {
            var field = form.FormFields.FirstOrDefault(f => f.Name == fieldValue.Key);
            if (field == null) continue;

            if (field.IsRequired && string.IsNullOrWhiteSpace(fieldValue.Value))
                return BadRequest($"Field '{field.Label}' is required");

            submission.AddValue(field.Id, field.Name, fieldValue.Value, "string");
        }

        foreach (var file in request.Files ?? new List<IFormFile>())
        {
            var fieldName = file.Name;
            var field = form.FormFields.FirstOrDefault(f => f.Name == fieldName && f.FieldType == FieldType.FileUpload);
            
            if (field == null) continue;

            var maxSize = form.Organization.Settings.MaxFileUploadSizeMB * 1024 * 1024;
            if (file.Length > maxSize)
                return BadRequest($"File size exceeds maximum allowed size of {form.Organization.Settings.MaxFileUploadSizeMB}MB");

            var (filePath, fileHash, fileSize) = await _fileStorage.SaveFileAsync(file, $"submissions/{submission.Id}");
            
            submission.AddFileAttachment(
                field.Id,
                file.FileName,
                fileSize,
                file.ContentType,
                filePath,
                fileHash
            );
        }

        if (form.ApprovalWorkflowId.HasValue)
        {
            submission.SubmitForApproval();
            await StartApprovalProcess(submission, form.ApprovalWorkflowId.Value);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            submissionId = submission.Id,
            trackingNumber = submission.TrackingNumber,
            message = "Form submitted successfully"
        });
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetSubmissions(int formId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Unauthorized();

        var form = await _context.Forms
            .FirstOrDefaultAsync(f => f.Id == formId && f.OrganizationId == _currentUser.OrganizationId);

        if (form == null)
            return NotFound("Form not found");

        if (!_currentUser.HasPermission("view_forms") && form.CreatedByUserId != _currentUser.UserId)
            return Forbid("Insufficient permissions");

        var query = _context.FormSubmissions
            .Where(s => s.FormId == formId)
            .Include(s => s.SubmittedByUser)
            .Include(s => s.SubmissionValues)
            .OrderByDescending(s => s.SubmittedAt);

        var totalCount = await query.CountAsync();
        var submissions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.TrackingNumber,
                s.Status,
                s.SubmittedAt,
                SubmittedBy = s.SubmittedByUser.FullName,
                Values = s.SubmissionValues.ToDictionary(sv => sv.FieldName, sv => sv.Value)
            })
            .ToListAsync();

        return Ok(new
        {
            items = submissions,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{submissionId}")]
    [Authorize]
    public async Task<IActionResult> GetSubmission(int formId, int submissionId)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Unauthorized();

        var submission = await _context.FormSubmissions
            .Include(s => s.Form)
            .Include(s => s.SubmittedByUser)
            .Include(s => s.SubmissionValues)
            .Include(s => s.FileAttachments)
            .Include(s => s.ApprovalProcesses)
                .ThenInclude(ap => ap.ApprovalActions)
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.FormId == formId);

        if (submission == null)
            return NotFound("Submission not found");

        if (submission.Form.OrganizationId != _currentUser.OrganizationId)
            return Forbid();

        var canView = _currentUser.HasPermission("view_forms") ||
                     submission.Form.CreatedByUserId == _currentUser.UserId ||
                     submission.SubmittedByUserId == _currentUser.UserId;

        if (!canView)
            return Forbid("Insufficient permissions");

        var result = new
        {
            submission.Id,
            submission.TrackingNumber,
            submission.Status,
            submission.SubmittedAt,
            SubmittedBy = submission.SubmittedByUser.FullName,
            Values = submission.SubmissionValues.ToDictionary(sv => sv.FieldName, sv => sv.Value),
            Files = submission.FileAttachments.Select(fa => new
            {
                fa.Id,
                fa.FileName,
                fa.FileSize,
                fa.ContentType,
                fa.UploadedAt
            }),
            ApprovalHistory = submission.ApprovalProcesses.SelectMany(ap => ap.ApprovalActions.Select(aa => new
            {
                aa.Action,
                aa.Comments,
                aa.ActionAt,
                ActionBy = aa.ActionByUser.FullName
            }))
        };

        return Ok(result);
    }

    private async Task<User> GetOrCreateGuestUser(int organizationId)
    {
        var guestEmail = $"guest-{Guid.NewGuid():N}@temporary.local";
        var (hash, salt) = HashPassword(Guid.NewGuid().ToString());

        var guestUser = new User(
            organizationId,
            guestEmail,
            "Guest",
            "User",
            hash,
            salt
        );

        _context.Users.Add(guestUser);
        await _context.SaveChangesAsync();

        return guestUser;
    }

    private async Task StartApprovalProcess(FormSubmission submission, int workflowId)
    {
        var workflow = await _context.ApprovalWorkflows
            .Include(w => w.ApprovalSteps)
            .FirstOrDefaultAsync(w => w.Id == workflowId);

        if (workflow == null || !workflow.IsActive)
            return;

        var firstStep = workflow.GetFirstStep();
        if (firstStep == null)
            return;

        var approvalProcess = new ApprovalProcess(submission.Id, workflowId);
        approvalProcess.StartProcess(firstStep.Id);

        _context.ApprovalProcesses.Add(approvalProcess);
    }

    private static (string hash, string salt) HashPassword(string password)
    {
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var saltBytes = new byte[32];
        rng.GetBytes(saltBytes);
        var salt = Convert.ToBase64String(saltBytes);

        using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(password, saltBytes, 10000, System.Security.Cryptography.HashAlgorithmName.SHA256);
        var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

        return (hash, salt);
    }
}

public class FormSubmissionRequest
{
    public Dictionary<string, string> FieldValues { get; set; } = new();
    public List<IFormFile>? Files { get; set; }
}