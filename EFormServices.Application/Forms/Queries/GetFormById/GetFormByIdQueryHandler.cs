// EFormServices.Application/Users/Queries/GetUserById/GetUserByIdQueryHandler.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EFormServices.Application.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUserByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Result<UserDto>.Failure("User not authenticated");

        var canViewAllUsers = _currentUser.HasPermission("view_users");
        var canViewOwnProfile = request.Id == _currentUser.UserId;

        if (!canViewAllUsers && !canViewOwnProfile)
            return Result<UserDto>.Failure("Insufficient permissions to view this user");

        var user = await _context.Users
            .Where(u => u.Id == request.Id && u.OrganizationId == _currentUser.OrganizationId)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
            return Result<UserDto>.Failure("User not found");

        return Result<UserDto>.Success(user);
    }
}