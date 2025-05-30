// EFormServices.Tests.Unit/Controllers/AuthControllerTests.cs
// Got code 30/05/2025
using EFormServices.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;

namespace EFormServices.Tests.Unit.Controllers;

public class AuthControllerTests
{
    private readonly AuthController _controller;
    private readonly IConfiguration _configuration;

    public AuthControllerTests()
    {
        var configData = new Dictionary<string, string>
        {
            ["Jwt:Key"] = "DevelopmentKeyThatIsAtLeast256BitsLongForDevelopmentOnly123456789012345678901234567890",
            ["Jwt:Issuer"] = "EFormServices",
            ["Jwt:Audience"] = "EFormServices",
            ["Jwt:ExpirationMinutes"] = "60"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        _controller = new AuthController(_configuration);
    }

    [Fact]
    public void Login_ValidCredentials_ReturnsOkWithToken()
    {
        var request = new LoginRequest
        {
            Email = "admin@demo.com",
            Password = "Admin123!"
        };

        var result = _controller.Login(request);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as LoginResponse;
        response!.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Email = "invalid@demo.com",
            Password = "wrongpassword"
        };

        var result = _controller.Login(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public void Login_EmptyCredentials_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Email = "",
            Password = ""
        };

        var result = _controller.Login(request);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Theory]
    [InlineData("admin@demo.com", "Admin123!")]
    [InlineData("manager@demo.com", "Manager123!")]
    [InlineData("user@demo.com", "User123!")]
    [InlineData("approval@demo.com", "Approve123!")]
    [InlineData("finance@demo.com", "Finance123!")]
    [InlineData("hr@demo.com", "HR123!")]
    public void Login_AllTestUsers_ReturnSuccess(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };

        var result = _controller.Login(request);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Logout_Always_ReturnsOk()
    {
        var result = _controller.Logout();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void RefreshToken_InvalidToken_ReturnsBadRequest()
    {
        var request = new RefreshTokenRequest
        {
            AccessToken = "invalid.token.here",
            RefreshToken = Guid.NewGuid().ToString()
        };

        var result = _controller.RefreshToken(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}