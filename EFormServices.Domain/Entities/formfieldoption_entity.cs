// EFormServices.Domain/Entities/formfieldoption_entity.cs
// Got code 30/05/2025
namespace EFormServices.Domain.Entities;

public class FormFieldOption : BaseEntity
{
    public int FormFieldId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsDefault { get; private set; }

    public FormField FormField { get; private set; } = null!;

    private FormFieldOption() 
    {
        Label = string.Empty;
        Value = string.Empty;
    }

    public FormFieldOption(int formFieldId, string label, string value, int sortOrder, bool isDefault = false)
    {
        FormFieldId = formFieldId;
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        SortOrder = sortOrder;
        IsDefault = isDefault;
        UpdateTimestamp();
    }

    public void UpdateOption(string label, string value)
    {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        UpdateTimestamp();
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdateTimestamp();
    }

    public void SetAsDefault()
    {
        if (!IsDefault)
        {
            IsDefault = true;
            UpdateTimestamp();
        }
    }

    public void RemoveDefault()
    {
        if (IsDefault)
        {
            IsDefault = false;
            UpdateTimestamp();
        }
    }
}