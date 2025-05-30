// EFormServices.Application/Organizations/Commands/CreateOrganization/CreateOrganizationCommandHandler.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using EFormServices.Domain.Entities;
using EFormServices.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Organizations.Commands.CreateOrganization;

public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Result<OrganizationDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateOrganizationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OrganizationDto>> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var existingOrg = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Subdomain == request.Subdomain.ToLowerInvariant(), cancellationToken);

        if (existingOrg != null)
            return Result<OrganizationDto>.Failure("Subdomain already exists");

        var settings = request.Settings != null 
            ? new OrganizationSettings(
                request.Settings.TimeZone,
                request.Settings.DateFormat,
                request.Settings.Currency,
                request.Settings.AllowPublicForms,
                request.Settings.MaxFileUploadSizeMB,
                request.Settings.FormRetentionDays,
                request.Settings.RequireApprovalForPublish)
            : OrganizationSettings.Default();

        var organization = new Organization(request.Name, request.Subdomain, settings);

        await _context.SaveChangesAsync(cancellationToken);

        var organizationDto = new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Subdomain = organization.Subdomain,
            TenantKey = organization.TenantKey,
            IsActive = organization.IsActive,
            CreatedAt = organization.CreatedAt,
            UpdatedAt = organization.UpdatedAt,
            Settings = new OrganizationSettingsDto
            {
                TimeZone = settings.TimeZone,
                DateFormat = settings.DateFormat,
                Currency = settings.Currency,
                AllowPublicForms = settings.AllowPublicForms,
                MaxFileUploadSizeMB = settings.MaxFileUploadSizeMB,
                FormRetentionDays = settings.FormRetentionDays,
                RequireApprovalForPublish = settings.RequireApprovalForPublish
            }
        };
        
        return Result<OrganizationDto>.Success(organizationDto);
    }
}