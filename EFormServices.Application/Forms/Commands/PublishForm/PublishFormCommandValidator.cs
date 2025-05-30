// EFormServices.Application/Forms/Commands/PublishForm/PublishFormCommandValidator.cs
// Got code 30/05/2025
using FluentValidation;

namespace EFormServices.Application.Forms.Commands.PublishForm;

public class PublishFormCommandValidator : AbstractValidator<PublishFormCommand>
{
    public PublishFormCommandValidator()
    {
        RuleFor(x => x.FormId)
            .GreaterThan(0).WithMessage("Form ID must be valid");
    }
}