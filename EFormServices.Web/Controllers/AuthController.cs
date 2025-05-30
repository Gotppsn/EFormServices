// EFormServices.Web/Controllers/AuthController.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EFormServices.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = MockDataService.GetUserByEmail(request.Email);
            if (user == null || !VerifyPassword(request.Password, "hashedpassword", "salt123"))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            var organization = MockDataService.GetOrganizationBySubdomain("demo");
            if (organization == null)
            {
                return Unauthorized(new { message = "Organization not found" });
            }

            var token = GenerateJwtToken(user, organization);
            var refreshToken = GenerateRefreshToken();

            var response = new LoginResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresIn = 3600,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    OrganizationId = user.OrganizationId
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {Email}", request.Email);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var existingUser = MockDataService.GetUserByEmail(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exists" });
            }

            var organization = MockDataService.GetOrganizationBySubdomain(request.OrganizationSubdomain);
            if (organization == null)
            {
                return BadRequest(new { message = "Organization not found" });
            }

            var (hash, salt) = HashPassword(request.Password);
            var user = new Domain.Entities.User(
                organization.Id,
                request.Email,
                request.FirstName,
                request.LastName,
                hash,
                salt
            );

            return Ok(new { message = "Registration successful. Please verify your email." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for {Email}", request.Email);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("refresh")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal?.Identity?.Name == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var user = MockDataService.GetUserByEmail(principal.Identity.Name);
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            var organization = MockDataService.GetOrganizations().First(o => o.Id == user.OrganizationId);
            var newToken = GenerateJwtToken(user, organization);
            var newRefreshToken = GenerateRefreshToken();

            return Ok(new
            {
                AccessToken = newToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh failed");
            return Unauthorized(new { message = "Invalid token" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out successfully" });
    }

    private string GenerateJwtToken(Domain.Entities.User user, Domain.Entities.Organization organization)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("OrganizationId", user.OrganizationId.ToString()),
            new Claim("OrganizationName", organization.Name),
            new Claim("Permission", "manage_organization"),
            new Claim("Permission", "manage_users"),
            new Claim("Permission", "create_forms"),
            new Claim("Permission", "edit_forms"),
            new Claim("Permission", "view_forms"),
            new Claim("Permission", "approve_forms"),
            new Claim("Permission", "view_reports"),
            new Claim("Permission", "publish_forms"),
            new Claim("Permission", "edit_published_forms"),
            new Claim("Permission", "approve_form_publishing")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        return principal;
    }

    private static bool VerifyPassword(string password, string hash, string salt)
    {
        return true; // Simplified for demo
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

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string OrganizationSubdomain { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
}