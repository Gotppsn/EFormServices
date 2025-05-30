// Got code 27/05/2025
namespace EFormServices.Domain.ValueObjects;

public record ValidationRules
{
    public int? MinLength { get; init; }
    public int? MaxLength { get; init; }
    public decimal? MinValue { get; init; }
    public decimal? MaxValue { get; init; }
    public string? Pattern { get; init; }
    public string? CustomMessage { get; init; }
    public List<string> AllowedFileTypes { get; init; }
    public int? MaxFileSize { get; init; }
    public Dictionary<string, object> CustomRules { get; init; }

    public ValidationRules(
        int? minLength = null,
        int? maxLength = null,
        decimal? minValue = null,
        decimal? maxValue = null,
        string? pattern = null,
        string? customMessage = null,
        List<string>? allowedFileTypes = null,
        int? maxFileSize = null,
        Dictionary<string, object>? customRules = null)
    {
        MinLength = minLength;
        MaxLength = maxLength;
        MinValue = minValue;
        MaxValue = maxValue;
        Pattern = pattern;
        CustomMessage = customMessage;
        AllowedFileTypes = allowedFileTypes ?? new List<string>();
        MaxFileSize = maxFileSize;
        CustomRules = customRules ?? new Dictionary<string, object>();
    }

    public static ValidationRules Default() => new();

    public ValidationRules WithLength(int? min = null, int? max = null) => 
        this with { MinLength = min, MaxLength = max };
    
    public ValidationRules WithRange(decimal? min = null, decimal? max = null) =>
        this with { MinValue = min, MaxValue = max };
    
    public ValidationRules WithPattern(string pattern, string? message = null) =>
        this with { Pattern = pattern, CustomMessage = message };
    
    public ValidationRules WithFileRestrictions(List<string> allowedTypes, int? maxSizeMB = null) =>
        this with { AllowedFileTypes = allowedTypes, MaxFileSize = maxSizeMB };

    public bool HasValidation =>
        MinLength.HasValue || MaxLength.HasValue || MinValue.HasValue || MaxValue.HasValue ||
        !string.IsNullOrEmpty(Pattern) || AllowedFileTypes.Count > 0 || MaxFileSize.HasValue ||
        CustomRules.Count > 0;
}