// EFormServices.Application/Common/DTOs/FormDto.cs
// Got code 30/05/2025
using EFormServices.Domain.Enums;

namespace EFormServices.Application.Common.DTOs;

public record FormDto
{
    public int Id { get; init; }
    public int OrganizationId { get; init; }
    public int? DepartmentId { get; init; }
    public int CreatedByUserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public FormType FormType { get; init; }
    public bool IsTemplate { get; init; }
    public bool IsActive { get; init; }
    public bool IsPublic { get; init; }
    public bool IsPublished { get; init; }
    public DateTime? PublishedAt { get; init; }
    public string FormKey { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string CreatedByUserName { get; init; } = string.Empty;
    public string? DepartmentName { get; init; }
    public int SubmissionCount { get; init; }
    public FormSettingsDto Settings { get; init; } = new();
    public FormMetadataDto Metadata { get; init; } = new();
}

public record FormSettingsDto
{
    public bool AllowMultipleSubmissions { get; init; }
    public bool RequireAuthentication { get; init; } = true;
    public bool ShowProgressBar { get; init; } = true;
    public bool AllowSaveAndContinue { get; init; }
    public bool ShowSubmissionNumber { get; init; } = true;
    public int? MaxSubmissions { get; init; }
    public DateTime? SubmissionStartDate { get; init; }
    public DateTime? SubmissionEndDate { get; init; }
    public string? RedirectUrl { get; init; }
    public string? SuccessMessage { get; init; }
}

public record FormMetadataDto
{
    public string Version { get; init; } = "1.0";
    public string? Category { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = new List<string>();
    public string? Language { get; init; } = "en";
    public int EstimatedCompletionMinutes { get; init; } = 5;
}