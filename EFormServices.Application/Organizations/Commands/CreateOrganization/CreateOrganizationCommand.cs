// EFormServices.Application/Organizations/Commands/CreateOrganization/CreateOrganizationCommand.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Models;
using MediatR;

namespace EFormServices.Application.Organizations.Commands.CreateOrganization;

public record CreateOrganizationCommand : IRequest<Result<OrganizationDto>>
{
    public string Name { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public OrganizationSettingsDto? Settings { get; init; }
}