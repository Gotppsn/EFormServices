// EFormServices.Application/FormFields/Commands/AddFormField/AddFormFieldCommandValidator.cs
// Got code 30/05/2025
using FluentValidation;

namespace EFormServices.Application.FormFields.Commands.AddFormField;

public class AddFormFieldCommandValidator : AbstractValidator<AddFormFieldCommand>
{
    public AddFormFieldCommandValidator()
    {
        RuleFor(x => x.FormId)
            .GreaterThan(0).WithMessage("Form ID must be valid");

        RuleFor(x => x.FieldType)
            .IsInEnum().WithMessage("Invalid field type");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required")
            .MaximumLength(100).WithMessage("Label cannot exceed 100 characters");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$").WithMessage("Name must start with a letter and contain only letters, numbers, and underscores");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.SortOrder)
            .GreaterThan(0).WithMessage("Sort order must be greater than 0");

        RuleForEach(x => x.Options)
            .ChildRules(option =>
            {
                option.RuleFor(o => o.Label)
                    .NotEmpty().WithMessage("Option label is required")
                    .MaximumLength(100).WithMessage("Option label cannot exceed 100 characters");

                option.RuleFor(o => o.Value)
                    .NotEmpty().WithMessage("Option value is required")
                    .MaximumLength(100).WithMessage("Option value cannot exceed 100 characters");
            });
    }
}