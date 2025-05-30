// EFormServices.Tests.Unit/Utilities/AuthTestUtilities.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace EFormServices.Tests.Unit.Utilities;

public static class AuthTestUtilities
{
    public static readonly Dictionary<string, string> TestUsers = new()
    {
        { "admin@demo.com", "Admin123!" },
        { "manager@demo.com", "Manager123!" },
        { "user@demo.com", "User123!" },
        { "approval@demo.com", "Approve123!" },
        { "finance@demo.com", "Finance123!" },
        { "hr@demo.com", "HR123!" }
    };

    public static IConfiguration GetTestConfiguration()
    {
        var configData = new Dictionary<string, string>
        {
            ["Jwt:Key"] = "DevelopmentKeyThatIsAtLeast256BitsLongForDevelopmentOnly123456789012345678901234567890",
            ["Jwt:Issuer"] = "EFormServices",
            ["Jwt:Audience"] = "EFormServices",
            ["Jwt:ExpirationMinutes"] = "60"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();
    }

    public static string GenerateTestJwtToken(int userId, string email, int organizationId, IConfiguration? config = null)
    {
        config ??= GetTestConfiguration();
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("OrganizationId", organizationId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static HttpContext CreateMockHttpContext(int userId, string email, int organizationId)
    {
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("OrganizationId", organizationId.ToString())
        };

        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        return context;
    }

    public static ClaimsPrincipal CreateTestUser(int userId = 1, string email = "admin@demo.com", int organizationId = 1)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("OrganizationId", organizationId.ToString()),
            new Claim("Permission", "manage_users"),
            new Claim("Permission", "create_forms"),
            new Claim("Permission", "edit_forms"),
            new Claim("Permission", "view_forms"),
            new Claim("Permission", "approve_forms")
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    public static class TestRoles
    {
        public const string Administrator = "Administrator";
        public const string Manager = "Manager";
        public const string User = "User";
        public const string Approver = "Approver";
    }

    public static class TestPermissions
    {
        public const string ManageUsers = "manage_users";
        public const string CreateForms = "create_forms";
        public const string EditForms = "edit_forms";
        public const string ViewForms = "view_forms";
        public const string ApproveForms = "approve_forms";
        public const string ManageOrganization = "manage_organization";
    }
}