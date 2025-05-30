// EFormServices.Application/Organizations/Commands/CreateOrganization/CreateOrganizationCommandValidator.cs
// Got code 30/05/2025
using FluentValidation;

namespace EFormServices.Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required")
            .MaximumLength(100).WithMessage("Organization name cannot exceed 100 characters");

        RuleFor(x => x.Subdomain)
            .NotEmpty().WithMessage("Subdomain is required")
            .MinimumLength(3).WithMessage("Subdomain must be at least 3 characters")
            .MaximumLength(50).WithMessage("Subdomain cannot exceed 50 characters")
            .Matches(@"^[a-z0-9-]+$").WithMessage("Subdomain can only contain lowercase letters, numbers, and hyphens");

        When(x => x.Settings != null, () =>
        {
            RuleFor(x => x.Settings!.MaxFileUploadSizeMB)
                .GreaterThan(0).WithMessage("Max file upload size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Max file upload size cannot exceed 100MB");

            RuleFor(x => x.Settings!.FormRetentionDays)
                .GreaterThan(0).WithMessage("Form retention days must be greater than 0")
                .LessThanOrEqualTo(3650).WithMessage("Form retention cannot exceed 10 years");
        });
    }
}