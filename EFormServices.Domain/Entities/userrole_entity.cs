// Got code 27/05/2025
namespace EFormServices.Domain.Entities;

public class UserRole : BaseEntity
{
    public int UserId { get; private set; }
    public int RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }

    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { }

    public UserRole(int userId, int roleId, DateTime? expiresAt = null)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IsActive = true;
        UpdateTimestamp();
    }

    public void ExtendExpiration(DateTime? newExpiresAt)
    {
        ExpiresAt = newExpiresAt;
        UpdateTimestamp();
    }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            UpdateTimestamp();
        }
    }

    public void Deactivate()
    {
        if (IsActive)
        {
            IsActive = false;
            UpdateTimestamp();
        }
    }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
    public bool IsValidRole => IsActive && !IsExpired;
}