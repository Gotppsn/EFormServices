// EFormServices.Application/Forms/Commands/UpdateForm/UpdateFormCommand.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Models;
using MediatR;

namespace EFormServices.Application.Forms.Commands.UpdateForm;

public record UpdateFormCommand : IRequest<Result<FormDto>>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public FormSettingsDto? Settings { get; init; }
    public FormMetadataDto? Metadata { get; init; }
}