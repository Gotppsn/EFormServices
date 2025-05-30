// Got code 27/05/2025
namespace EFormServices.Domain.Enums;

public enum ConditionalOperator
{
    Equals = 1,
    NotEquals = 2,
    Contains = 3,
    NotContains = 4,
    GreaterThan = 5,
    LessThan = 6,
    GreaterThanOrEqual = 7,
    LessThanOrEqual = 8,
    IsEmpty = 9,
    IsNotEmpty = 10,
    StartsWith = 11,
    EndsWith = 12
}

public enum ConditionalAction
{
    Show = 1,
    Hide = 2,
    Enable = 3,
    Disable = 4,
    Require = 5,
    Optional = 6,
    SetValue = 7,
    ClearValue = 8
}