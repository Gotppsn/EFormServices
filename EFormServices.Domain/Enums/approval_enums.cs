// Got code 27/05/2025
namespace EFormServices.Domain.Enums;

public enum ApprovalStatus
{
    Pending = 1,
    InProgress = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5,
    Expired = 6
}

public enum ApprovalActionType
{
    Approve = 1,
    Reject = 2,
    RequestChanges = 3,
    Delegate = 4,
    Skip = 5,
    Comment = 6
}