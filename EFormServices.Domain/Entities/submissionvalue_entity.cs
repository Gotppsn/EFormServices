// Got code 27/05/2025
namespace EFormServices.Domain.Entities;

public class SubmissionValue : BaseEntity
{
    public int FormSubmissionId { get; private set; }
    public int FormFieldId { get; private set; }
    public string FieldName { get; private set; }
    public string Value { get; private set; }
    public string ValueType { get; private set; }

    public FormSubmission FormSubmission { get; private set; } = null!;
    public FormField FormField { get; private set; } = null!;

    private SubmissionValue() { }

    public SubmissionValue(int formSubmissionId, int formFieldId, string fieldName, string value, string valueType)
    {
        FormSubmissionId = formSubmissionId;
        FormFieldId = formFieldId;
        FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
        Value = value ?? string.Empty;
        ValueType = valueType ?? "string";
        UpdateTimestamp();
    }

    public void UpdateValue(string value, string valueType)
    {
        Value = value ?? string.Empty;
        ValueType = valueType ?? "string";
        UpdateTimestamp();
    }

    public T GetTypedValue<T>()
    {
        return ValueType switch
        {
            "int" => (T)(object)int.Parse(Value),
            "decimal" => (T)(object)decimal.Parse(Value),
            "bool" => (T)(object)bool.Parse(Value),
            "datetime" => (T)(object)DateTime.Parse(Value),
            "string" => (T)(object)Value,
            _ => (T)(object)Value
        };
    }

    public bool HasValue => !string.IsNullOrWhiteSpace(Value);
}