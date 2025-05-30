// Got code 27/05/2025
namespace EFormServices.Domain.ValueObjects;

public record WorkflowSettings
{
    public bool AutoStartWorkflow { get; init; }
    public bool AllowSkipSteps { get; init; }
    public bool RequireComments { get; init; }
    public int DefaultTimeoutHours { get; init; }
    public bool NotifyOnTimeout { get; init; }
    public bool AllowReassignment { get; init; }
    public bool EscalateOnTimeout { get; init; }
    public Dictionary<string, object> CustomSettings { get; init; }

    public WorkflowSettings(
        bool autoStartWorkflow = true,
        bool allowSkipSteps = false,
        bool requireComments = false,
        int defaultTimeoutHours = 24,
        bool notifyOnTimeout = true,
        bool allowReassignment = true,
        bool escalateOnTimeout = false,
        Dictionary<string, object>? customSettings = null)
    {
        AutoStartWorkflow = autoStartWorkflow;
        AllowSkipSteps = allowSkipSteps;
        RequireComments = requireComments;
        DefaultTimeoutHours = defaultTimeoutHours;
        NotifyOnTimeout = notifyOnTimeout;
        AllowReassignment = allowReassignment;
        EscalateOnTimeout = escalateOnTimeout;
        CustomSettings = customSettings ?? new Dictionary<string, object>();
    }

    public static WorkflowSettings Default() => new();

    public WorkflowSettings WithTimeout(int hours) => this with { DefaultTimeoutHours = hours };
    public WorkflowSettings WithComments(bool required = true) => this with { RequireComments = required };
    public WorkflowSettings WithEscalation(bool escalate = true) => this with { EscalateOnTimeout = escalate };
}