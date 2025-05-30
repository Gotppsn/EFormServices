// EFormServices.Web/Controllers/FormsController.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EFormServices.Infrastructure.Services;
using EFormServices.Domain.Entities;
using EFormServices.Domain.Enums;
using EFormServices.Domain.ValueObjects;
using System.Security.Claims;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/forms")]
[Authorize]
public class FormsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetForms(
        int page = 1, 
        int pageSize = 20, 
        string? searchTerm = null,
        FormType? formType = null,
        bool? isPublished = null,
        string sortBy = "createdAt",
        bool sortDescending = true)
    {
        var organizationId = GetOrganizationId();
        var forms = MockDataService.GetForms()
            .Where(f => f.OrganizationId == organizationId);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            forms = forms.Where(f => f.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                   (f.Description != null && f.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }

        if (formType.HasValue)
            forms = forms.Where(f => f.FormType == formType.Value);

        if (isPublished.HasValue)
            forms = forms.Where(f => f.IsPublished == isPublished.Value);

        var totalCount = forms.Count();
        var sortedForms = sortBy.ToLowerInvariant() switch
        {
            "title" => sortDescending ? forms.OrderByDescending(f => f.Title) : forms.OrderBy(f => f.Title),
            "updatedat" => sortDescending ? forms.OrderByDescending(f => f.UpdatedAt) : forms.OrderBy(f => f.UpdatedAt),
            _ => sortDescending ? forms.OrderByDescending(f => f.CreatedAt) : forms.OrderBy(f => f.CreatedAt)
        };

        var pagedForms = sortedForms
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new
            {
                id = f.Id,
                title = f.Title,
                description = f.Description,
                formType = f.FormType,
                isPublished = f.IsPublished,
                isActive = f.IsActive,
                formKey = f.FormKey,
                submissionCount = f.SubmissionCount,
                createdAt = f.CreatedAt,
                updatedAt = f.UpdatedAt,
                createdByUserName = "User"
            })
            .ToList();

        return Ok(new
        {
            items = pagedForms,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public IActionResult GetForm(int id)
    {
        var organizationId = GetOrganizationId();
        var form = MockDataService.GetForms().FirstOrDefault(f => f.Id == id && f.OrganizationId == organizationId);

        if (form == null)
            return NotFound();

        var fields = MockDataService.GetFormFields().Where(f => f.FormId == id).ToList();

        return Ok(new
        {
            id = form.Id,
            title = form.Title,
            description = form.Description,
            formType = form.FormType,
            isPublished = form.IsPublished,
            isActive = form.IsActive,
            formKey = form.FormKey,
            fields = fields.Select(f => new
            {
                id = f.Id,
                fieldType = f.FieldType,
                label = f.Label,
                name = f.Name,
                description = f.Description,
                isRequired = f.IsRequired,
                sortOrder = f.SortOrder,
                settings = new { },
                validationRules = new { },
                options = new List<object>()
            })
        });
    }

    [HttpGet("public/{formKey}")]
    [AllowAnonymous]
    public IActionResult GetPublicForm(string formKey)
    {
        var form = MockDataService.GetForms().FirstOrDefault(f => f.FormKey == formKey && f.IsPublished);

        if (form == null)
            return NotFound();

        var fields = MockDataService.GetFormFields().Where(f => f.FormId == form.Id).ToList();

        return Ok(new
        {
            id = form.Id,
            title = form.Title,
            description = form.Description,
            formKey = form.FormKey,
            settings = new
            {
                allowMultipleSubmissions = true,
                requireAuthentication = false,
                showProgressBar = true,
                allowSaveAndContinue = false,
                showSubmissionNumber = true,
                successMessage = "Thank you for your submission!"
            },
            metadata = new
            {
                version = "1.0",
                estimatedCompletionMinutes = 5
            },
            fields = fields.Select(f => new
            {
                id = f.Id,
                fieldType = f.FieldType,
                label = f.Label,
                name = f.Name,
                description = f.Description,
                isRequired = f.IsRequired,
                sortOrder = f.SortOrder,
                settings = new
                {
                    placeholder = "",
                    defaultValue = "",
                    isReadOnly = false,
                    isVisible = true,
                    rows = f.FieldType == FieldType.TextArea ? 4 : (int?)null
                },
                validationRules = new
                {
                    minLength = (int?)null,
                    maxLength = (int?)null,
                    pattern = (string?)null,
                    allowedFileTypes = new List<string>(),
                    maxFileSize = (int?)null
                },
                options = new List<object>()
            })
        });
    }

    [HttpPost]
    [Authorize(Policy = "CreateForms")]
    public IActionResult CreateForm([FromBody] CreateFormRequest request)
    {
        var organizationId = GetOrganizationId();
        var userId = GetUserId();

        var form = new Form(organizationId, userId, request.Title, request.FormType, request.Description);
        MockDataService.AddForm(form);

        return CreatedAtAction(nameof(GetForm), new { id = form.Id }, new
        {
            id = form.Id,
            title = form.Title,
            description = form.Description,
            formType = form.FormType,
            formKey = form.FormKey,
            isPublished = form.IsPublished,
            createdAt = form.CreatedAt
        });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "EditForms")]
    public IActionResult UpdateForm(int id, [FromBody] UpdateFormRequest request)
    {
        var organizationId = GetOrganizationId();
        var form = MockDataService.GetForms().FirstOrDefault(f => f.Id == id && f.OrganizationId == organizationId);

        if (form == null)
            return NotFound();

        form.UpdateDetails(request.Title, request.Description);
        MockDataService.UpdateForm(form);

        return Ok(new
        {
            id = form.Id,
            title = form.Title,
            description = form.Description,
            updatedAt = form.UpdatedAt
        });
    }

    [HttpPost("{id}/publish")]
    [Authorize(Policy = "EditForms")]
    public IActionResult PublishForm(int id)
    {
        var organizationId = GetOrganizationId();
        var form = MockDataService.GetForms().FirstOrDefault(f => f.Id == id && f.OrganizationId == organizationId);

        if (form == null)
            return NotFound();

        if (form.IsPublished)
            return BadRequest(new { message = "Form is already published" });

        form.Publish();
        MockDataService.UpdateForm(form);

        return Ok(new { message = "Form published successfully" });
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "EditForms")]
    public IActionResult DeleteForm(int id)
    {
        var organizationId = GetOrganizationId();
        var form = MockDataService.GetForms().FirstOrDefault(f => f.Id == id && f.OrganizationId == organizationId);

        if (form == null)
            return NotFound();

        MockDataService.DeleteForm(id);
        return NoContent();
    }

    [HttpPost("submit")]
    [AllowAnonymous]
    public IActionResult SubmitForm([FromBody] SubmitFormRequest request)
    {
        var form = MockDataService.GetForms().FirstOrDefault(f => f.FormKey == request.FormKey);

        if (form == null)
            return NotFound();

        var submission = new FormSubmission(form.Id, 1);
        var trackingNumber = $"F{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{Random.Shared.Next(1000, 9999)}";

        return Ok(new
        {
            success = true,
            trackingNumber,
            message = "Form submitted successfully!"
        });
    }

    [HttpGet("analytics")]
    public IActionResult GetAnalytics()
    {
        var organizationId = GetOrganizationId();
        var forms = MockDataService.GetForms().Where(f => f.OrganizationId == organizationId).ToList();
        var submissions = MockDataService.GetSubmissions().ToList();

        return Ok(new
        {
            totalForms = forms.Count,
            activeForms = forms.Count(f => f.IsActive),
            totalSubmissions = submissions.Count,
            pendingApprovals = 0,
            topForms = forms.Take(5).Select(f => new
            {
                title = f.Title,
                submissions = submissions.Count(s => s.FormId == f.Id)
            })
        });
    }

    private int GetOrganizationId()
    {
        var orgId = User.FindFirst("OrganizationId")?.Value;
        return int.Parse(orgId ?? "1");
    }

    private int GetUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userId ?? "1");
    }

    public record CreateFormRequest(string Title, string? Description, FormType FormType);
    public record UpdateFormRequest(string Title, string? Description);
    public record SubmitFormRequest(string FormKey, Dictionary<string, object> SubmissionData);
}