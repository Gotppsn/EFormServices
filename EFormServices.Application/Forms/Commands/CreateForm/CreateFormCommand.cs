// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Models;
using EFormServices.Domain.Enums;
using MediatR;

namespace EFormServices.Application.Forms.Commands.CreateForm;

public record CreateFormCommand : IRequest<Result<FormDto>>
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public FormType FormType { get; init; }
    public int? DepartmentId { get; init; }
    public int? ApprovalWorkflowId { get; init; }
    public FormSettingsDto? Settings { get; init; }
    public FormMetadataDto? Metadata { get; init; }
}