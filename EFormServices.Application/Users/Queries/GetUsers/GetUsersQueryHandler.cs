// EFormServices.Application/Users/Queries/GetUsers/GetUsersQueryHandler.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUsersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Result<PagedResult<UserDto>>.Failure("User not authenticated");

        if (!_currentUser.HasPermission("view_users"))
            return Result<PagedResult<UserDto>>.Failure("Insufficient permissions to view users");

        var query = _context.Users
            .Where(u => u.OrganizationId == _currentUser.OrganizationId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(u => u.FirstName.Contains(request.SearchTerm) ||
                                   u.LastName.Contains(request.SearchTerm) ||
                                   u.Email.Contains(request.SearchTerm));
        }

        if (request.DepartmentId.HasValue)
            query = query.Where(u => u.DepartmentId == request.DepartmentId);

        if (request.IsActive.HasValue)
            query = query.Where(u => u.IsActive == request.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy.ToLowerInvariant() switch
        {
            "firstname" => request.SortDescending ? query.OrderByDescending(u => u.FirstName) : query.OrderBy(u => u.FirstName),
            "email" => request.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "createdat" => request.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
            "lastloginat" => request.SortDescending ? query.OrderByDescending(u => u.LastLoginAt) : query.OrderBy(u => u.LastLoginAt),
            _ => request.SortDescending ? query.OrderByDescending(u => u.LastName) : query.OrderBy(u => u.LastName)
        };

        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserDto
            {
                Id = u.Id,
                OrganizationId = u.OrganizationId,
                DepartmentId = u.DepartmentId,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                IsActive = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt,
                DepartmentName = null,
                Roles = new List<string>()
            })
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<UserDto>(users, totalCount, request.Page, request.PageSize);
        return Result<PagedResult<UserDto>>.Success(pagedResult);
    }
}