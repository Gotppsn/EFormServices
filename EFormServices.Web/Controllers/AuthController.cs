// EFormServices.Web/Controllers/AuthController.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json;
using EFormServices.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = MockDataService.GetUserByEmail(request.Email);
        if (user == null)
        {
            return BadRequest(new { message = "Invalid email or password" });
        }

        if (!VerifyPassword(request.Password, user.PasswordHash))
        {
            return BadRequest(new { message = "Invalid email or password" });
        }

        var organization = MockDataService.GetOrganizations().First(o => o.Id == user.OrganizationId);
        var roles = MockDataService.GetRoles().Where(r => r.OrganizationId == user.OrganizationId).ToList();
        var permissions = MockDataService.GetPermissions().ToList();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("OrganizationId", user.OrganizationId.ToString()),
            new("OrganizationName", organization.Name),
            new("UserId", user.Id.ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission.Name));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
            new ClaimsPrincipal(claimsIdentity), authProperties);

        var accessToken = GenerateAccessToken(claims);
        var refreshToken = GenerateRefreshToken();

        user.RecordLogin();

        return Ok(new
        {
            accessToken,
            refreshToken,
            user = new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                organizationId = user.OrganizationId,
                organizationName = organization.Name
            }
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        if (!User.Identity?.IsAuthenticated == true)
        {
            return Unauthorized();
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var user = MockDataService.GetUserById(int.Parse(userId));
        if (user == null)
        {
            return NotFound();
        }

        var organization = MockDataService.GetOrganizations().First(o => o.Id == user.OrganizationId);

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            organizationId = user.OrganizationId,
            organizationName = organization.Name
        });
    }

    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return BadRequest(new { message = "Invalid access token" });
        }

        var newAccessToken = GenerateAccessToken(principal.Claims);
        var newRefreshToken = GenerateRefreshToken();

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken
        });
    }

    private string GenerateAccessToken(IEnumerable<Claim> claims)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? 
            "DevelopmentKeyThatIsAtLeast256BitsLongForDevelopmentOnly123456789012345678901234567890"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60")),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? 
            "DevelopmentKeyThatIsAtLeast256BitsLongForDevelopmentOnly123456789012345678901234567890"));

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return password == "password123" || hash == "hashedpassword";
    }

    public record LoginRequest(string Email, string Password);
    public record RefreshTokenRequest(string AccessToken, string RefreshToken);
}