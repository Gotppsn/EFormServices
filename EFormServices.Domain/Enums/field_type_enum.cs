// Got code 27/05/2025
namespace EFormServices.Domain.Enums;

public enum FieldType
{
    Text = 1,
    Email = 2,
    Number = 3,
    Phone = 4,
    TextArea = 5,
    Dropdown = 6,
    RadioButton = 7,
    Checkbox = 8,
    Date = 9,
    Time = 10,
    DateTime = 11,
    FileUpload = 12,
    Signature = 13,
    Rating = 14,
    Boolean = 15,
    Currency = 16,
    Url = 17,
    Password = 18,
    Hidden = 19,
    Section = 20
}

public static class FieldTypeExtensions
{
    public static bool SupportsOptions(this FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.Dropdown => true,
            FieldType.RadioButton => true,
            FieldType.Checkbox => true,
            FieldType.Rating => true,
            _ => false
        };
    }

    public static bool SupportsMultipleValues(this FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.Checkbox => true,
            FieldType.FileUpload => true,
            _ => false
        };
    }

    public static bool RequiresValidation(this FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.Email => true,
            FieldType.Number => true,
            FieldType.Phone => true,
            FieldType.Date => true,
            FieldType.Time => true,
            FieldType.DateTime => true,
            FieldType.Currency => true,
            FieldType.Url => true,
            _ => false
        };
    }

    public static string GetInputType(this FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.Text => "text",
            FieldType.Email => "email",
            FieldType.Number => "number",
            FieldType.Phone => "tel",
            FieldType.Date => "date",
            FieldType.Time => "time",
            FieldType.DateTime => "datetime-local",
            FieldType.Password => "password",
            FieldType.Url => "url",
            FieldType.Hidden => "hidden",
            FieldType.Boolean => "checkbox",
            _ => "text"
        };
    }
}