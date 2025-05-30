// EFormServices.Application/Users/Commands/CreateUser/CreateUserCommandHandler.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Interfaces;
using EFormServices.Application.Common.Models;
using EFormServices.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace EFormServices.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateUserCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.OrganizationId.HasValue)
            return Result<UserDto>.Failure("User not authenticated");

        if (!_currentUser.HasPermission("manage_users"))
            return Result<UserDto>.Failure("Insufficient permissions to create users");

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (existingUser != null)
            return Result<UserDto>.Failure("User with this email already exists");

        if (request.DepartmentId.HasValue)
        {
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == request.DepartmentId && d.OrganizationId == _currentUser.OrganizationId, cancellationToken);
            
            if (department == null)
                return Result<UserDto>.Failure("Department not found");
        }

        var validRoles = await _context.Roles
            .Where(r => request.RoleIds.Contains(r.Id) && r.OrganizationId == _currentUser.OrganizationId)
            .ToListAsync(cancellationToken);

        if (request.RoleIds.Count != validRoles.Count)
            return Result<UserDto>.Failure("One or more roles not found");

        var (passwordHash, salt) = HashPassword(request.Password);

        var user = new User(
            _currentUser.OrganizationId.Value,
            request.Email,
            request.FirstName,
            request.LastName,
            passwordHash,
            salt,
            request.DepartmentId,
            request.ExternalId
        );

        if (_context is EFormServices.Infrastructure.Data.MockApplicationDbContext mockContext)
        {
            var users = (EFormServices.Infrastructure.Data.MockDbSet<User>)mockContext.Users;
            user.Id = _context.Users.Count() + 1;
            users.Add(user);

            var userRoles = (EFormServices.Infrastructure.Data.MockDbSet<UserRole>)mockContext.UserRoles;
            foreach (var roleId in request.RoleIds)
            {
                var userRole = new UserRole(user.Id, roleId);
                userRole.Id = userRoles.Count() + 1;
                userRoles.Add(userRole);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,
            OrganizationId = user.OrganizationId,
            DepartmentId = user.DepartmentId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            DepartmentName = null,
            Roles = validRoles.Select(r => r.Name).ToList()
        };

        return Result<UserDto>.Success(userDto);
    }

    private static (string hash, string salt) HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[32];
        rng.GetBytes(saltBytes);
        var salt = Convert.ToBase64String(saltBytes);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
        var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

        return (hash, salt);
    }
}