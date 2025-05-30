// EFormServices.Application/Common/DTOs/UserDto.cs
// Got code 30/05/2025
namespace EFormServices.Application.Common.DTOs;

public record UserDto
{
    public int Id { get; init; }
    public int OrganizationId { get; init; }
    public int? DepartmentId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public bool EmailConfirmed { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? DepartmentName { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = new List<string>();
}