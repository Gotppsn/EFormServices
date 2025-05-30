// Got code 27/05/2025
using EFormServices.Domain.Enums;
using EFormServices.Domain.ValueObjects;

namespace EFormServices.Domain.Entities;

public class ApprovalWorkflow : BaseEntity
{
    public int OrganizationId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public WorkflowType WorkflowType { get; private set; }
    public bool IsActive { get; private set; }
    public WorkflowSettings Settings { get; private set; }

    public Organization Organization { get; private set; } = null!;

    private readonly List<ApprovalStep> _approvalSteps = new();
    private readonly List<Form> _forms = new();

    public IReadOnlyCollection<ApprovalStep> ApprovalSteps => _approvalSteps.AsReadOnly();
    public IReadOnlyCollection<Form> Forms => _forms.AsReadOnly();

    private ApprovalWorkflow() { }

    public ApprovalWorkflow(int organizationId, string name, WorkflowType workflowType, 
                           string? description = null, WorkflowSettings? settings = null)
    {
        OrganizationId = organizationId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        WorkflowType = workflowType;
        IsActive = true;
        Settings = settings ?? WorkflowSettings.Default();
        UpdateTimestamp();
    }

    public void UpdateDetails(string name, string? description = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        UpdateTimestamp();
    }

    public void UpdateSettings(WorkflowSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        UpdateTimestamp();
    }

    public void AddStep(string stepName, StepType stepType, string approverCriteria, 
                       bool requireAllApprovers = false, int timeoutHours = 24)
    {
        var stepOrder = _approvalSteps.Count + 1;
        _approvalSteps.Add(new ApprovalStep(Id, stepName, stepType, stepOrder, 
                                           approverCriteria, requireAllApprovers, timeoutHours));
        UpdateTimestamp();
    }

    public void RemoveStep(int stepId)
    {
        var step = _approvalSteps.FirstOrDefault(s => s.Id == stepId);
        if (step != null)
        {
            _approvalSteps.Remove(step);
            ReorderSteps();
            UpdateTimestamp();
        }
    }

    public void ReorderSteps()
    {
        var orderedSteps = _approvalSteps.OrderBy(s => s.StepOrder).ToList();
        for (int i = 0; i < orderedSteps.Count; i++)
        {
            orderedSteps[i].UpdateStepOrder(i + 1);
        }
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdateTimestamp();
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdateTimestamp();
        }
    }

    public ApprovalStep? GetFirstStep()
    {
        return _approvalSteps.OrderBy(s => s.StepOrder).FirstOrDefault();
    }

    public ApprovalStep? GetNextStep(int currentStepOrder)
    {
        return _approvalSteps.OrderBy(s => s.StepOrder)
                            .FirstOrDefault(s => s.StepOrder > currentStepOrder);
    }

    public bool HasSteps => _approvalSteps.Count > 0;
    public int StepCount => _approvalSteps.Count;
}