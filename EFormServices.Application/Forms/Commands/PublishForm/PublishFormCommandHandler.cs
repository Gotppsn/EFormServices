// EFormServices.Application/Forms/Commands/PublishForm/PublishFormCommandHandler.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Forms.Commands.PublishForm;

public class PublishFormCommandHandler : IRequestHandler<PublishFormCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PublishFormCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(PublishFormCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Result.Failure("User not authenticated");

        var form = await _context.Forms
            .Include(f => f.FormFields)
            .FirstOrDefaultAsync(f => f.Id == request.FormId && f.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (form == null)
            return Result.Failure("Form not found");

        if (!_currentUser.HasPermission("publish_forms") && form.CreatedByUserId != _currentUser.UserId)
            return Result.Failure("Insufficient permissions to publish this form");

        if (form.IsPublished)
            return Result.Failure("Form is already published");

        if (!form.FormFields.Any())
            return Result.Failure("Cannot publish form without fields");

        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == _currentUser.OrganizationId, cancellationToken);

        if (organization?.Settings.RequireApprovalForPublish == true && !_currentUser.HasPermission("approve_form_publishing"))
            return Result.Failure("Form publishing requires approval");

        form.Publish();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}