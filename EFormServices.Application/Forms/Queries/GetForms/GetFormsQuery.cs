// EFormServices.Application/Forms/Queries/GetForms/GetFormsQuery.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Models;
using EFormServices.Domain.Enums;
using MediatR;

namespace EFormServices.Application.Forms.Queries.GetForms;

public record GetFormsQuery : IRequest<Result<PagedResult<FormDto>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public FormType? FormType { get; init; }
    public int? DepartmentId { get; init; }
    public bool? IsTemplate { get; init; }
    public bool? IsPublished { get; init; }
    public bool? IsActive { get; init; }
    public string SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;
}