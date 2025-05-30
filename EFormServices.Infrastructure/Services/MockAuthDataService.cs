// EFormServices.Infrastructure/Services/MockAuthDataService.cs
// Got code 30/05/2025
using EFormServices.Domain.Entities;
using EFormServices.Domain.Enums;
using EFormServices.Domain.ValueObjects;
using System.Security.Cryptography;

namespace EFormServices.Infrastructure.Services;

public static class MockAuthDataService
{
    private static readonly Dictionary<string, string> _userCredentials = new()
    {
        { "admin@demo.com", "Admin123!" },
        { "manager@demo.com", "Manager123!" },
        { "user@demo.com", "User123!" },
        { "approval@demo.com", "Approve123!" },
        { "finance@demo.com", "Finance123!" },
        { "hr@demo.com", "HR123!" }
    };

    public static Dictionary<string, string> GetTestCredentials() => _userCredentials;

    public static bool ValidateCredentials(string email, string password)
    {
        return _userCredentials.TryGetValue(email.ToLowerInvariant(), out var storedPassword) 
               && storedPassword == password;
    }

    public static User? GetUserByCredentials(string email, string password)
    {
        if (!ValidateCredentials(email, password))
            return null;

        var users = MockDataService.GetUsers();
        return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public static (string hash, string salt) HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var saltBytes = new byte[32];
        rng.GetBytes(saltBytes);
        var salt = Convert.ToBase64String(saltBytes);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
        var hash = Convert.ToBase64String(pbkdf2.GetBytes(32));

        return (hash, salt);
    }

    public static bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
        var testHash = Convert.ToBase64String(pbkdf2.GetBytes(32));
        return testHash == hash;
    }

    public static List<User> GetUsersWithHashedPasswords()
    {
        var users = new List<User>();
        var org = MockDataService.GetOrganizations().First();

        foreach (var credential in _userCredentials)
        {
            var (hash, salt) = HashPassword(credential.Value);
            var nameParts = credential.Key.Split('@')[0].Split('.');
            var firstName = char.ToUpper(nameParts[0][0]) + nameParts[0][1..];
            var lastName = nameParts.Length > 1 ? char.ToUpper(nameParts[1][0]) + nameParts[1][1..] : "User";

            var user = new User(
                org.Id,
                credential.Key,
                firstName,
                lastName,
                hash,
                salt
            )
            {
                Id = users.Count + 1
            };

            users.Add(user);
        }

        return users;
    }
}