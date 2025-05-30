// EFormServices.Application/Forms/Commands/CreateForm/CreateFormCommandValidator.cs
// Got code 30/05/2025
using FluentValidation;

namespace EFormServices.Application.Forms.Commands.CreateForm;

public class CreateFormCommandValidator : AbstractValidator<CreateFormCommand>
{
    public CreateFormCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.FormType)
            .IsInEnum().WithMessage("Invalid form type");

        When(x => x.Settings != null, () =>
        {
            RuleFor(x => x.Settings!.MaxSubmissions)
                .GreaterThan(0).When(x => x.Settings!.MaxSubmissions.HasValue)
                .WithMessage("Max submissions must be greater than 0");

            RuleFor(x => x.Settings!.SubmissionStartDate)
                .LessThan(x => x.Settings!.SubmissionEndDate)
                .When(x => x.Settings!.SubmissionStartDate.HasValue && x.Settings!.SubmissionEndDate.HasValue)
                .WithMessage("Submission start date must be before end date");
        });

        When(x => x.Metadata != null, () =>
        {
            RuleFor(x => x.Metadata!.EstimatedCompletionMinutes)
                .GreaterThan(0).WithMessage("Estimated completion time must be greater than 0");

            RuleFor(x => x.Metadata!.Version)
                .NotEmpty().WithMessage("Version is required")
                .Matches(@"^\d+\.\d+$").WithMessage("Version must be in format x.y");
        });
    }
}