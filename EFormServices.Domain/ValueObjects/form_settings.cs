// Got code 27/05/2025
namespace EFormServices.Domain.ValueObjects;

public record FormSettings
{
    public bool AllowMultipleSubmissions { get; init; }
    public bool RequireAuthentication { get; init; }
    public bool ShowProgressBar { get; init; }
    public bool AllowSaveAndContinue { get; init; }
    public bool ShowSubmissionNumber { get; init; }
    public int? MaxSubmissions { get; init; }
    public DateTime? SubmissionStartDate { get; init; }
    public DateTime? SubmissionEndDate { get; init; }
    public string? RedirectUrl { get; init; }
    public string? SuccessMessage { get; init; }
    public Dictionary<string, object> CustomSettings { get; init; }

    public FormSettings(
        bool allowMultipleSubmissions = false,
        bool requireAuthentication = true,
        bool showProgressBar = true,
        bool allowSaveAndContinue = false,
        bool showSubmissionNumber = true,
        int? maxSubmissions = null,
        DateTime? submissionStartDate = null,
        DateTime? submissionEndDate = null,
        string? redirectUrl = null,
        string? successMessage = null,
        Dictionary<string, object>? customSettings = null)
    {
        AllowMultipleSubmissions = allowMultipleSubmissions;
        RequireAuthentication = requireAuthentication;
        ShowProgressBar = showProgressBar;
        AllowSaveAndContinue = allowSaveAndContinue;
        ShowSubmissionNumber = showSubmissionNumber;
        MaxSubmissions = maxSubmissions;
        SubmissionStartDate = submissionStartDate;
        SubmissionEndDate = submissionEndDate;
        RedirectUrl = redirectUrl;
        SuccessMessage = successMessage;
        CustomSettings = customSettings ?? new Dictionary<string, object>();
    }

    public static FormSettings Default() => new();

    public FormSettings WithMultipleSubmissions(bool allow = true) => this with { AllowMultipleSubmissions = allow };
    public FormSettings WithSubmissionLimit(int maxSubmissions) => this with { MaxSubmissions = maxSubmissions };
    public FormSettings WithSubmissionPeriod(DateTime startDate, DateTime endDate) => 
        this with { SubmissionStartDate = startDate, SubmissionEndDate = endDate };

    public bool IsSubmissionAllowed(DateTime now) =>
        (SubmissionStartDate == null || now >= SubmissionStartDate) &&
        (SubmissionEndDate == null || now <= SubmissionEndDate);
}