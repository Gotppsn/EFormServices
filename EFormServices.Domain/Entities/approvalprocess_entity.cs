// Got code 27/05/2025
using EFormServices.Domain.Enums;

namespace EFormServices.Domain.Entities;

public class ApprovalProcess : BaseEntity
{
    public int FormSubmissionId { get; private set; }
    public int ApprovalWorkflowId { get; private set; }
    public int? CurrentStepId { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Comments { get; private set; }

    public FormSubmission FormSubmission { get; private set; } = null!;
    public ApprovalWorkflow ApprovalWorkflow { get; private set; } = null!;
    public ApprovalStep? CurrentStep { get; private set; }

    private readonly List<ApprovalAction> _approvalActions = new();
    public IReadOnlyCollection<ApprovalAction> ApprovalActions => _approvalActions.AsReadOnly();

    private ApprovalProcess() { }

    public ApprovalProcess(int formSubmissionId, int approvalWorkflowId)
    {
        FormSubmissionId = formSubmissionId;
        ApprovalWorkflowId = approvalWorkflowId;
        Status = ApprovalStatus.Pending;
        StartedAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void StartProcess(int firstStepId)
    {
        if (Status == ApprovalStatus.Pending)
        {
            CurrentStepId = firstStepId;
            Status = ApprovalStatus.InProgress;
            UpdateTimestamp();
        }
    }

    public void MoveToNextStep(int nextStepId)
    {
        if (Status == ApprovalStatus.InProgress)
        {
            CurrentStepId = nextStepId;
            UpdateTimestamp();
        }
    }

    public void CompleteProcess()
    {
        if (Status == ApprovalStatus.InProgress)
        {
            Status = ApprovalStatus.Approved;
            CompletedAt = DateTime.UtcNow;
            CurrentStepId = null;
            UpdateTimestamp();
        }
    }

    public void RejectProcess(string comments)
    {
        if (Status == ApprovalStatus.InProgress)
        {
            Status = ApprovalStatus.Rejected;
            CompletedAt = DateTime.UtcNow;
            Comments = comments;
            CurrentStepId = null;
            UpdateTimestamp();
        }
    }

    public void CancelProcess()
    {
        if (Status == ApprovalStatus.InProgress || Status == ApprovalStatus.Pending)
        {
            Status = ApprovalStatus.Cancelled;
            CompletedAt = DateTime.UtcNow;
            CurrentStepId = null;
            UpdateTimestamp();
        }
    }

    public void AddAction(int approvalStepId, int actionByUserId, ApprovalActionType action, string? comments = null)
    {
        _approvalActions.Add(new ApprovalAction(Id, approvalStepId, actionByUserId, action, comments));
        UpdateTimestamp();
    }

    public bool IsCompleted => CompletedAt.HasValue;
    public bool IsInProgress => Status == ApprovalStatus.InProgress;
    public bool IsApproved => Status == ApprovalStatus.Approved;
    public bool IsRejected => Status == ApprovalStatus.Rejected;

    public TimeSpan ProcessingTime
    {
        get
        {
            var endTime = CompletedAt ?? DateTime.UtcNow;
            return endTime - StartedAt;
        }
    }
}