// Got code 27/05/2025
using EFormServices.Domain.Enums;

namespace EFormServices.Domain.Entities;

public class ApprovalStep : BaseEntity
{
    public int ApprovalWorkflowId { get; private set; }
    public string StepName { get; private set; }
    public StepType StepType { get; private set; }
    public int StepOrder { get; private set; }
    public string ApproverCriteria { get; private set; }
    public bool RequireAllApprovers { get; private set; }
    public int TimeoutHours { get; private set; }

    public ApprovalWorkflow ApprovalWorkflow { get; private set; } = null!;

    private readonly List<ApprovalProcess> _approvalProcesses = new();
    public IReadOnlyCollection<ApprovalProcess> ApprovalProcesses => _approvalProcesses.AsReadOnly();

    private ApprovalStep() { }

    public ApprovalStep(int approvalWorkflowId, string stepName, StepType stepType, int stepOrder,
                       string approverCriteria, bool requireAllApprovers = false, int timeoutHours = 24)
    {
        ApprovalWorkflowId = approvalWorkflowId;
        StepName = stepName ?? throw new ArgumentNullException(nameof(stepName));
        StepType = stepType;
        StepOrder = stepOrder;
        ApproverCriteria = approverCriteria ?? throw new ArgumentNullException(nameof(approverCriteria));
        RequireAllApprovers = requireAllApprovers;
        TimeoutHours = timeoutHours;
        UpdateTimestamp();
    }

    public void UpdateDetails(string stepName, string approverCriteria)
    {
        StepName = stepName ?? throw new ArgumentNullException(nameof(stepName));
        ApproverCriteria = approverCriteria ?? throw new ArgumentNullException(nameof(approverCriteria));
        UpdateTimestamp();
    }

    public void UpdateStepOrder(int stepOrder)
    {
        StepOrder = stepOrder;
        UpdateTimestamp();
    }

    public void UpdateTimeout(int timeoutHours)
    {
        TimeoutHours = timeoutHours;
        UpdateTimestamp();
    }

    public void SetRequireAllApprovers(bool requireAll)
    {
        RequireAllApprovers = requireAll;
        UpdateTimestamp();
    }

    public bool IsFirstStep => StepOrder == 1;
    public bool IsTimeout(DateTime startTime) => DateTime.UtcNow > startTime.AddHours(TimeoutHours);
}