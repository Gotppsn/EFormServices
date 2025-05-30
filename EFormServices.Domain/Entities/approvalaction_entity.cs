// Got code 27/05/2025
using EFormServices.Domain.Enums;

namespace EFormServices.Domain.Entities;

public class ApprovalAction : BaseEntity
{
    public int ApprovalProcessId { get; private set; }
    public int ApprovalStepId { get; private set; }
    public int ActionByUserId { get; private set; }
    public ApprovalActionType Action { get; private set; }
    public string? Comments { get; private set; }
    public DateTime ActionAt { get; private set; }

    public ApprovalProcess ApprovalProcess { get; private set; } = null!;
    public ApprovalStep ApprovalStep { get; private set; } = null!;
    public User ActionByUser { get; private set; } = null!;

    private ApprovalAction() { }

    public ApprovalAction(int approvalProcessId, int approvalStepId, int actionByUserId, 
                         ApprovalActionType action, string? comments = null)
    {
        ApprovalProcessId = approvalProcessId;
        ApprovalStepId = approvalStepId;
        ActionByUserId = actionByUserId;
        Action = action;
        Comments = comments;
        ActionAt = DateTime.UtcNow;
        UpdateTimestamp();
    }

    public void UpdateComments(string comments)
    {
        Comments = comments;
        UpdateTimestamp();
    }

    public bool IsApprovalAction => Action == ApprovalActionType.Approve;
    public bool IsRejectionAction => Action == ApprovalActionType.Reject;
    public bool HasComments => !string.IsNullOrWhiteSpace(Comments);
}