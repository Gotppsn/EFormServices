// Got code 27/05/2025
namespace EFormServices.Domain.Enums;

public enum WorkflowType
{
    Sequential = 1,
    Parallel = 2,
    Conditional = 3,
    Hybrid = 4
}

public enum StepType
{
    UserApproval = 1,
    RoleApproval = 2,
    DepartmentApproval = 3,
    SystemApproval = 4,
    ExternalApproval = 5
}