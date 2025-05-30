// EFormServices.Application/FormFields/Commands/AddFormField/AddFormFieldCommandHandler.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using EFormServices.Domain.Entities;
using EFormServices.Domain.ValueObjects;
using EFormServices.Domain.Enums;
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
            .FirstOrDefaultAsync(f => f.Id == request.FormId && f.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (form == null)
            return Result<int>.Failure("Form not found");

        if (!_currentUser.HasPermission("edit_forms") && form.CreatedByUserId != _currentUser.UserId)
            return Result<int>.Failure("Insufficient permissions to modify this form");

        if (form.IsPublished && !_currentUser.HasPermission("edit_published_forms"))
            return Result<int>.Failure("Cannot modify published form");

        var existingFields = await _context.FormFields
            .Where(f => f.FormId == request.FormId)
            .ToListAsync(cancellationToken);

        if (existingFields.Any(f => f.Name == request.Name))
            return Result<int>.Failure("Field with this name already exists");

        var validationRules = CreateValidationRules(request.ValidationRules);
        var fieldSettings = CreateFieldSettings(request.FieldType, request.Settings);

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

        if (_context is EFormServices.Infrastructure.Data.MockApplicationDbContext mockContext)
        {
            var fields = (EFormServices.Infrastructure.Data.MockDbSet<FormField>)mockContext.FormFields;
            formField.Id = _context.FormFields.Count() + 1;
            fields.Add(formField);

            if (request.Options != null && request.FieldType.SupportsOptions())
            {
                var options = (EFormServices.Infrastructure.Data.MockDbSet<FormFieldOption>)mockContext.FormFieldOptions;
                foreach (var option in request.Options)
                {
                    var fieldOption = new FormFieldOption(formField.Id, option.Label, option.Value, 
                        options.Count(o => o.FormFieldId == formField.Id) + 1, option.IsDefault);
                    fieldOption.Id = options.Count() + 1;
                    options.Add(fieldOption);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(formField.Id);
    }

    private static ValidationRules CreateValidationRules(Dictionary<string, object>? validationRules)
    {
        if (validationRules == null) return ValidationRules.Default();

        return new ValidationRules(
            validationRules.TryGetValue("MinLength", out var minLength) ? Convert.ToInt32(minLength) : null,
            validationRules.TryGetValue("MaxLength", out var maxLength) ? Convert.ToInt32(maxLength) : null,
            validationRules.TryGetValue("MinValue", out var minValue) ? Convert.ToDecimal(minValue) : null,
            validationRules.TryGetValue("MaxValue", out var maxValue) ? Convert.ToDecimal(maxValue) : null,
            validationRules.TryGetValue("Pattern", out var pattern) ? pattern.ToString() : null
        );
    }

    private static FieldSettings CreateFieldSettings(FieldType fieldType, Dictionary<string, object>? settings)
    {
        if (settings == null) return FieldSettings.Default(fieldType);

        return new FieldSettings(
            settings.TryGetValue("Placeholder", out var placeholder) ? placeholder.ToString() : null,
            settings.TryGetValue("DefaultValue", out var defaultValue) ? defaultValue.ToString() : null,
            settings.TryGetValue("IsReadOnly", out var isReadOnly) && Convert.ToBoolean(isReadOnly),
            !settings.TryGetValue("IsVisible", out var isVisible) || Convert.ToBoolean(isVisible),
            settings.TryGetValue("Rows", out var rows) ? Convert.ToInt32(rows) : null,
            settings.TryGetValue("Columns", out var columns) ? Convert.ToInt32(columns) : null,
            settings.TryGetValue("AllowMultiple", out var allowMultiple) && Convert.ToBoolean(allowMultiple)
        );
    }
}