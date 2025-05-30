// Got code 27/05/2025
namespace EFormServices.Domain.ValueObjects;

public record OrganizationSettings
{
    public string TimeZone { get; init; }
    public string DateFormat { get; init; }
    public string Currency { get; init; }
    public bool AllowPublicForms { get; init; }
    public int MaxFileUploadSizeMB { get; init; }
    public int FormRetentionDays { get; init; }
    public bool RequireApprovalForPublish { get; init; }
    public Dictionary<string, object> CustomSettings { get; init; }

    public OrganizationSettings(
        string timeZone = "UTC",
        string dateFormat = "yyyy-MM-dd",
        string currency = "USD",
        bool allowPublicForms = false,
        int maxFileUploadSizeMB = 10,
        int formRetentionDays = 365,
        bool requireApprovalForPublish = true,
        Dictionary<string, object>? customSettings = null)
    {
        TimeZone = timeZone;
        DateFormat = dateFormat;
        Currency = currency;
        AllowPublicForms = allowPublicForms;
        MaxFileUploadSizeMB = maxFileUploadSizeMB;
        FormRetentionDays = formRetentionDays;
        RequireApprovalForPublish = requireApprovalForPublish;
        CustomSettings = customSettings ?? new Dictionary<string, object>();
    }

    public static OrganizationSettings Default() => new();

    public OrganizationSettings WithTimeZone(string timeZone) => this with { TimeZone = timeZone };
    public OrganizationSettings WithFileUploadLimit(int sizeMB) => this with { MaxFileUploadSizeMB = sizeMB };
    public OrganizationSettings WithRetentionPolicy(int days) => this with { FormRetentionDays = days };
}