// Got code 27/05/2025
using EFormServices.Domain.Enums;

namespace EFormServices.Domain.ValueObjects;

public record FieldSettings
{
    public string? Placeholder { get; init; }
    public string? DefaultValue { get; init; }
    public bool IsReadOnly { get; init; }
    public bool IsVisible { get; init; }
    public int? Rows { get; init; }
    public int? Columns { get; init; }
    public bool AllowMultiple { get; init; }
    public int? Step { get; init; }
    public string? CssClass { get; init; }
    public Dictionary<string, object> Attributes { get; init; }

    public FieldSettings(
        string? placeholder = null,
        string? defaultValue = null,
        bool isReadOnly = false,
        bool isVisible = true,
        int? rows = null,
        int? columns = null,
        bool allowMultiple = false,
        int? step = null,
        string? cssClass = null,
        Dictionary<string, object>? attributes = null)
    {
        Placeholder = placeholder;
        DefaultValue = defaultValue;
        IsReadOnly = isReadOnly;
        IsVisible = isVisible;
        Rows = rows;
        Columns = columns;
        AllowMultiple = allowMultiple;
        Step = step;
        CssClass = cssClass;
        Attributes = attributes ?? new Dictionary<string, object>();
    }

    public static FieldSettings Default(FieldType fieldType = FieldType.Text)
    {
        return fieldType switch
        {
            FieldType.TextArea => new FieldSettings(rows: 4, columns: 50),
            FieldType.Number => new FieldSettings(step: 1),
            FieldType.Currency => new FieldSettings(step: 1),
            FieldType.FileUpload => new FieldSettings(allowMultiple: false),
            _ => new FieldSettings()
        };
    }

    public FieldSettings WithPlaceholder(string placeholder) => this with { Placeholder = placeholder };
    public FieldSettings WithDefaultValue(string defaultValue) => this with { DefaultValue = defaultValue };
    public FieldSettings WithReadOnly(bool isReadOnly = true) => this with { IsReadOnly = isReadOnly };
    public FieldSettings WithVisibility(bool isVisible = true) => this with { IsVisible = isVisible };
    public FieldSettings WithDimensions(int rows, int columns) => this with { Rows = rows, Columns = columns };
    public FieldSettings WithMultiple(bool allowMultiple = true) => this with { AllowMultiple = allowMultiple };
}