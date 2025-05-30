// EFormServices.Application/Common/DTOs/OrganizationDto.cs
// Got code 30/05/2025
namespace EFormServices.Application.Common.DTOs;

public record OrganizationDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public string TenantKey { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public OrganizationSettingsDto Settings { get; init; } = new();
}

public record OrganizationSettingsDto
{
    public string TimeZone { get; init; } = "UTC";
    public string DateFormat { get; init; } = "yyyy-MM-dd";
    public string Currency { get; init; } = "USD";
    public bool AllowPublicForms { get; init; }
    public int MaxFileUploadSizeMB { get; init; } = 10;
    public int FormRetentionDays { get; init; } = 365;
    public bool RequireApprovalForPublish { get; init; } = true;
}