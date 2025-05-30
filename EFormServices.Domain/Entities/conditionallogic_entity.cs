// Got code 27/05/2025
using EFormServices.Domain.Enums;

namespace EFormServices.Domain.Entities;

public class ConditionalLogic : BaseEntity  
{
    public int FormId { get; private set; }
    public int TriggerFieldId { get; private set; }
    public int TargetFieldId { get; private set; }
    public ConditionalOperator Condition { get; private set; }
    public string TriggerValue { get; private set; }
    public ConditionalAction Action { get; private set; }

    public Form Form { get; private set; } = null!;
    public FormField TriggerField { get; private set; } = null!;
    public FormField TargetField { get; private set; } = null!;

    private ConditionalLogic() { }

    public ConditionalLogic(int formId, int triggerFieldId, int targetFieldId, 
                           ConditionalOperator condition, string triggerValue, ConditionalAction action)
    {
        FormId = formId;
        TriggerFieldId = triggerFieldId;
        TargetFieldId = targetFieldId;
        Condition = condition;
        TriggerValue = triggerValue ?? throw new ArgumentNullException(nameof(triggerValue));
        Action = action;
        UpdateTimestamp();
    }

    public void UpdateCondition(ConditionalOperator condition, string triggerValue)
    {
        Condition = condition;
        TriggerValue = triggerValue ?? throw new ArgumentNullException(nameof(triggerValue));
        UpdateTimestamp();
    }

    public void UpdateAction(ConditionalAction action)
    {
        Action = action;
        UpdateTimestamp();
    }

    public bool EvaluateCondition(string actualValue)
    {
        if (string.IsNullOrEmpty(actualValue))
            return Condition == ConditionalOperator.IsEmpty;

        return Condition switch
        {
            ConditionalOperator.Equals => actualValue.Equals(TriggerValue, StringComparison.OrdinalIgnoreCase),
            ConditionalOperator.NotEquals => !actualValue.Equals(TriggerValue, StringComparison.OrdinalIgnoreCase),
            ConditionalOperator.Contains => actualValue.Contains(TriggerValue, StringComparison.OrdinalIgnoreCase),
            ConditionalOperator.NotContains => !actualValue.Contains(TriggerValue, StringComparison.OrdinalIgnoreCase),
            ConditionalOperator.GreaterThan => CompareNumeric(actualValue, TriggerValue) > 0,
            ConditionalOperator.LessThan => CompareNumeric(actualValue, TriggerValue) < 0,
            ConditionalOperator.IsEmpty => string.IsNullOrWhiteSpace(actualValue),
            ConditionalOperator.IsNotEmpty => !string.IsNullOrWhiteSpace(actualValue),
            _ => false
        };
    }

    private static int CompareNumeric(string value1, string value2)
    {
        if (decimal.TryParse(value1, out var num1) && decimal.TryParse(value2, out var num2))
            return num1.CompareTo(num2);
        return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase);
    }
}