// EFormServices.Application/Users/Queries/GetUsers/GetUsersQuery.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Models;
using MediatR;

namespace EFormServices.Application.Users.Queries.GetUsers;

public record GetUsersQuery : IRequest<Result<PagedResult<UserDto>>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public int? DepartmentId { get; init; }
    public bool? IsActive { get; init; }
    public string SortBy { get; init; } = "LastName";
    public bool SortDescending { get; init; } = false;
}