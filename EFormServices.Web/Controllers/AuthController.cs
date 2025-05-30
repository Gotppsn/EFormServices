// EFormServices.Web/Controllers/AuthController.cs
// Got code 30/05/2025
using EFormServices.Application.Common.Interfaces;
using EFormServices.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Organization)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());

        if (user == null || !user.IsActive || !VerifyPassword(request.Password, user.PasswordHash, user.Salt))
        {
            return BadRequest(new { message = "Invalid credentials" });
        }

        if (!user.Organization.IsActive)
        {
            return BadRequest(new { message = "Organization is inactive" });
        }

        user.RecordLogin();
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return Ok(new LoginResponse
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
                OrganizationId = user.OrganizationId,
                OrganizationName = user.Organization.Name
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());
        if (existingUser != null)
        {
            return BadRequest(new { message = "User already exists" });
        }

        var organization = await _context.Organizations.FirstOrDefaultAsync(o => o.Subdomain == request.OrganizationSubdomain);
        if (organization == null || !organization.IsActive)
        {
            return BadRequest(new { message = "Invalid organization" });
        }

        var (passwordHash, salt) = HashPassword(request.Password);

        var user = new Domain.Entities.User(
            organization.Id,
            request.Email,
            request.FirstName,
            request.LastName,
            passwordHash,
            salt
        );

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.OrganizationId == organization.Id && r.Name == "User");
        if (defaultRole != null)
        {
            var userRole = new Domain.Entities.UserRole(user.Id, defaultRole.Id);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "User registered successfully" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
        {
            return BadRequest(new { message = "Invalid token" });
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(new { message = "Invalid token" });
        }

        var user = await _context.Users
            .Include(u => u.Organization)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || !user.IsActive)
        {
            return BadRequest(new { message = "Invalid token" });
        }

        var newToken = GenerateJwtToken(user);
        var newRefreshToken = GenerateRefreshToken();

        return Ok(new LoginResponse
        {
            AccessToken = newToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = 3600
        });
    }

    private string GenerateJwtToken(Domain.Entities.User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("OrganizationId", user.OrganizationId.ToString())
        };

        var activeRoles = user.UserRoles.Where(ur => ur.IsActive && !ur.IsExpired).ToList();
        foreach (var userRole in activeRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
            
            foreach (var rolePermission in userRole.Role.RolePermissions)
            {
                claims.Add(new Claim("Permission", rolePermission.Permission.Name));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256);
        var computedHash = Convert.ToBase64String(pbkdf2.GetBytes(32));
        return computedHash == hash;
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

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string OrganizationSubdomain { get; init; } = string.Empty;
}

public record RefreshTokenRequest
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}

public record LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
    public UserInfo? User { get; init; }
}

public record UserInfo
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public int OrganizationId { get; init; }
    public string OrganizationName { get; init; } = string.Empty;
}