// Got code 27/05/2025
namespace EFormServices.Domain.ValueObjects;

public record FormMetadata
{
    public string Version { get; init; }
    public string? Category { get; init; }
    public List<string> Tags { get; init; }
    public string? Language { get; init; }
    public int EstimatedCompletionMinutes { get; init; }
    public Dictionary<string, string> CustomAttributes { get; init; }

    public FormMetadata(
        string version = "1.0",
        string? category = null,
        List<string>? tags = null,
        string? language = "en",
        int estimatedCompletionMinutes = 5,
        Dictionary<string, string>? customAttributes = null)
    {
        Version = version;
        Category = category;
        Tags = tags ?? new List<string>();
        Language = language;
        EstimatedCompletionMinutes = estimatedCompletionMinutes;
        CustomAttributes = customAttributes ?? new Dictionary<string, string>();
    }

    public static FormMetadata Default() => new();

    public FormMetadata WithVersion(string version) => this with { Version = version };
    public FormMetadata WithCategory(string category) => this with { Category = category };
    public FormMetadata WithTags(params string[] tags) => this with { Tags = tags.ToList() };
    public FormMetadata WithEstimatedTime(int minutes) => this with { EstimatedCompletionMinutes = minutes };
}