// EFormServices.Application/Forms/Commands/UpdateForm/UpdateFormCommandHandler.cs
// Got code 30/05/2025
using AutoMapper;
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using EFormServices.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Forms.Commands.UpdateForm;

public class UpdateFormCommandHandler : IRequestHandler<UpdateFormCommand, Result<FormDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;

    public UpdateFormCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IMapper mapper)
    {
        _context = context;
        _currentUser = currentUser;
        _mapper = mapper;
    }

    public async Task<Result<FormDto>> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Result<FormDto>.Failure("User not authenticated");

        var form = await _context.Forms
            .FirstOrDefaultAsync(f => f.Id == request.Id && f.OrganizationId == _currentUser.OrganizationId, cancellationToken);

        if (form == null)
            return Result<FormDto>.Failure("Form not found");

        if (!_currentUser.HasPermission("edit_forms") && form.CreatedByUserId != _currentUser.UserId)
            return Result<FormDto>.Failure("Insufficient permissions to update this form");

        if (form.IsPublished && !_currentUser.HasPermission("edit_published_forms"))
            return Result<FormDto>.Failure("Cannot modify published form");

        form.UpdateDetails(request.Title, request.Description);

        if (request.Settings != null)
        {
            var settings = new FormSettings(
                request.Settings.AllowMultipleSubmissions,
                request.Settings.RequireAuthentication,
                request.Settings.ShowProgressBar,
                request.Settings.AllowSaveAndContinue,
                request.Settings.ShowSubmissionNumber,
                request.Settings.MaxSubmissions,
                request.Settings.SubmissionStartDate,
                request.Settings.SubmissionEndDate,
                request.Settings.RedirectUrl,
                request.Settings.SuccessMessage
            );
            form.UpdateSettings(settings);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var formDto = _mapper.Map<FormDto>(form);
        return Result<FormDto>.Success(formDto);
    }
}