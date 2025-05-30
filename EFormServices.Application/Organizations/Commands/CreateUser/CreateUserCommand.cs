// EFormServices.Application/Users/Commands/CreateUser/CreateUserCommand.cs
// Got code 30/05/2025
using EFormServices.Application.Common.DTOs;
using EFormServices.Application.Common.Models;
using MediatR;

namespace EFormServices.Application.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<Result<UserDto>>
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int? DepartmentId { get; init; }
    public List<int> RoleIds { get; init; } = new();
    public string? ExternalId { get; init; }
}