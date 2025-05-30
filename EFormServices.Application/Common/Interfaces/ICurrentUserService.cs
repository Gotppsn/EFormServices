// EFormServices.Application/Common/Interfaces/ICurrentUserService.cs
// Got code 30/05/2025
namespace EFormServices.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    int? OrganizationId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool HasPermission(string permission);
    bool HasRole(string role);
}