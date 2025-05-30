// Got code 27/05/2025
using EFormServices.Domain.Enums;
using EFormServices.Domain.ValueObjects;

namespace EFormServices.Domain.Entities;

public class FormField : BaseEntity
{
    public int FormId { get; private set; }
    public FieldType FieldType { get; private set; }
    public string Label { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public ValidationRules ValidationRules { get; private set; }
    public bool IsRequired { get; private set; }
    public int SortOrder { get; private set; }
    public FieldSettings Settings { get; private set; }

    public Form Form { get; private set; } = null!;

    private readonly List<FormFieldOption> _options = new();
    private readonly List<ConditionalLogic> _conditionalLogics = new();

    public IReadOnlyCollection<FormFieldOption> Options => _options.AsReadOnly();
    public IReadOnlyCollection<ConditionalLogic> ConditionalLogics => _conditionalLogics.AsReadOnly();

    private FormField() { }

    public FormField(int formId, FieldType fieldType, string label, string name,
                     int sortOrder, bool isRequired = false, string? description = null)
    {
        FormId = formId;
        FieldType = fieldType;
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        IsRequired = isRequired;
        SortOrder = sortOrder;
        ValidationRules = ValidationRules.Default();
        Settings = FieldSettings.Default(fieldType);
        UpdateTimestamp();
    }

    public void UpdateBasicInfo(string label, string name, string? description = null)
    {
        Label = label ?? throw new ArgumentNullException(nameof(label));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description;
        UpdateTimestamp();
    }

    public void UpdateValidation(ValidationRules validationRules, bool isRequired)
    {
        ValidationRules = validationRules ?? throw new ArgumentNullException(nameof(validationRules));
        IsRequired = isRequired;
        UpdateTimestamp();
    }

    public void UpdateSettings(FieldSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        UpdateTimestamp();
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdateTimestamp();
    }

    public void AddOption(string label, string value, bool isDefault = false)
    {
        if (!FieldType.SupportsOptions())
            throw new InvalidOperationException($"Field type {FieldType} does not support options");

        var sortOrder = _options.Count + 1;
        _options.Add(new FormFieldOption(Id, label, value, sortOrder, isDefault));
        UpdateTimestamp();
    }

    public void RemoveOption(int optionId)
    {
        var option = _options.FirstOrDefault(o => o.Id == optionId);
        if (option != null)
        {
            _options.Remove(option);
            UpdateTimestamp();
        }
    }

    public void ClearOptions()
    {
        _options.Clear();
        UpdateTimestamp();
    }

    public bool HasOptions => _options.Count > 0;
    public bool SupportsMultipleValues => FieldType.SupportsMultipleValues();
}