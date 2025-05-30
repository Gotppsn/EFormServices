// EFormServices.Application/FormFields/Commands/AddFormField/AddFormFieldCommandHandler.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using EFormServices.Domain.Entities;
using EFormServices.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.FormFields.Commands.AddFormField;

public class AddFormFieldCommandHandler : IRequestHandler<AddFormFieldCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddFormFieldCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<int>> Handle(AddFormFieldCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Result<int>.Failure("User not authenticated");

        var form = await _context.Forms
            .Include(f => f.FormFields)
            .FirstOrDefaultAsync(f => f.Id == request.FormId && f.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (form == null)
            return Result<int>.Failure("Form not found");

        if (!_currentUser.HasPermission("edit_forms") && form.CreatedByUserId != _currentUser.UserId)
            return Result<int>.Failure("Insufficient permissions to modify this form");

        if (form.IsPublished && !_currentUser.HasPermission("edit_published_forms"))
            return Result<int>.Failure("Cannot modify published form");

        var existingField = form.FormFields.FirstOrDefault(f => f.Name == request.Name);
        if (existingField != null)
            return Result<int>.Failure("Field with this name already exists");

        var validationRules = ValidationRules.Default();
        if (request.ValidationRules != null)
        {
            validationRules = new ValidationRules(
                request.ValidationRules.ContainsKey("MinLength") ? (int?)request.ValidationRules["MinLength"] : null,
                request.ValidationRules.ContainsKey("MaxLength") ? (int?)request.ValidationRules["MaxLength"] : null,
                request.ValidationRules.ContainsKey("MinValue") ? (decimal?)request.ValidationRules["MinValue"] : null,
                request.ValidationRules.ContainsKey("MaxValue") ? (decimal?)request.ValidationRules["MaxValue"] : null,
                request.ValidationRules.ContainsKey("Pattern") ? request.ValidationRules["Pattern"]?.ToString() : null
            );
        }

        var fieldSettings = FieldSettings.Default(request.FieldType);
        if (request.Settings != null)
        {
            fieldSettings = new FieldSettings(
                request.Settings.ContainsKey("Placeholder") ? request.Settings["Placeholder"]?.ToString() : null,
                request.Settings.ContainsKey("DefaultValue") ? request.Settings["DefaultValue"]?.ToString() : null,
                request.Settings.ContainsKey("IsReadOnly") && (bool)request.Settings["IsReadOnly"],
                !request.Settings.ContainsKey("IsVisible") || (bool)request.Settings["IsVisible"],
                request.Settings.ContainsKey("Rows") ? (int?)request.Settings["Rows"] : null,
                request.Settings.ContainsKey("Columns") ? (int?)request.Settings["Columns"] : null,
                request.Settings.ContainsKey("AllowMultiple") && (bool)request.Settings["AllowMultiple"]
            );
        }

        var formField = new FormField(
            request.FormId,
            request.FieldType,
            request.Label,
            request.Name,
            request.SortOrder,
            request.IsRequired,
            request.Description
        );

        formField.UpdateValidation(validationRules, request.IsRequired);
        formField.UpdateSettings(fieldSettings);

        form.AddField(formField);

        if (request.Options != null && request.FieldType.SupportsOptions())
        {
            var sortOrder = 1;
            foreach (var option in request.Options)
            {
                formField.AddOption(option.Label, option.Value, option.IsDefault);
                sortOrder++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(formField.Id);
    }
}