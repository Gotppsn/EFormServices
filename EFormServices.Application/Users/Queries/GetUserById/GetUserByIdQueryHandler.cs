// EFormServices.Application/Forms/Queries/GetFormById/GetFormByIdQueryHandler.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Forms.Queries.GetFormById;

public class GetFormByIdQueryHandler : IRequestHandler<GetFormByIdQuery, Result<FormDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetFormByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<FormDto>> Handle(GetFormByIdQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Forms.AsQueryable();

        if (_currentUser.IsAuthenticated && _currentUser.OrganizationId.HasValue)
        {
            query = query.Where(f => f.OrganizationId == _currentUser.OrganizationId);

            if (!_currentUser.HasPermission("view_all_forms"))
            {
                query = query.Where(f => f.CreatedByUserId == _currentUser.UserId || f.IsPublic);
            }
        }
        else
        {
            query = query.Where(f => f.IsPublic);
        }

        var form = await query
            .Where(f => f.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (form == null)
            return Result<FormDto>.Failure("Form not found");

        var formDto = new FormDto
        {
            Id = form.Id,
            OrganizationId = form.OrganizationId,
            DepartmentId = form.DepartmentId,
            CreatedByUserId = form.CreatedByUserId,
            Title = form.Title,
            Description = form.Description,
            FormType = form.FormType,
            IsTemplate = form.IsTemplate,
            IsActive = form.IsActive,
            IsPublic = form.IsPublic,
            IsPublished = form.IsPublished,
            PublishedAt = form.PublishedAt,
            FormKey = form.FormKey,
            CreatedAt = form.CreatedAt,
            UpdatedAt = form.UpdatedAt,
            CreatedByUserName = "User",
            DepartmentName = null,
            SubmissionCount = form.SubmissionCount,
            Settings = new FormSettingsDto
            {
                AllowMultipleSubmissions = form.Settings.AllowMultipleSubmissions,
                RequireAuthentication = form.Settings.RequireAuthentication,
                ShowProgressBar = form.Settings.ShowProgressBar,
                AllowSaveAndContinue = form.Settings.AllowSaveAndContinue,
                ShowSubmissionNumber = form.Settings.ShowSubmissionNumber,
                MaxSubmissions = form.Settings.MaxSubmissions,
                SubmissionStartDate = form.Settings.SubmissionStartDate,
                SubmissionEndDate = form.Settings.SubmissionEndDate,
                RedirectUrl = form.Settings.RedirectUrl,
                SuccessMessage = form.Settings.SuccessMessage
            },
            Metadata = new FormMetadataDto
            {
                Version = form.Metadata.Version,
                Category = form.Metadata.Category,
                Tags = form.Metadata.Tags.ToList(),
                Language = form.Metadata.Language,
                EstimatedCompletionMinutes = form.Metadata.EstimatedCompletionMinutes
            }
        };

        return Result<FormDto>.Success(formDto);
    }
}