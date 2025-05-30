// EFormServices.Tests.Integration/AuthenticationIntegrationTests.cs
// Got code 30/05/2025
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using Xunit;
using FluentAssertions;
using EFormServices.Web.Controllers;
using System.Text.Json;

namespace EFormServices.Tests.Integration;

public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsSuccessWithToken()
    {
        var request = new LoginRequest
        {
            Email = "admin@demo.com",
            Password = "Admin123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        loginResponse.Should().NotBeNull();
        loginResponse!.AccessToken.Should().NotBeNullOrEmpty();
        loginResponse.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var request = new LoginRequest
        {
            Email = "invalid@demo.com",
            Password = "wrongpassword"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("admin@demo.com", "Admin123!")]
    [InlineData("manager@demo.com", "Manager123!")]
    [InlineData("user@demo.com", "User123!")]
    [InlineData("approval@demo.com", "Approve123!")]
    [InlineData("finance@demo.com", "Finance123!")]
    [InlineData("hr@demo.com", "HR123!")]
    public async Task Login_AllTestAccounts_AuthenticateSuccessfully(string email, string password)
    {
        var request = new LoginRequest { Email = email, Password = password };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_CaseInsensitiveEmail_ReturnsSuccess()
    {
        var request = new LoginRequest
        {
            Email = "ADMIN@DEMO.COM",
            Password = "Admin123!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthenticatedRequest_ValidToken_ReturnsSuccess()
    {
        var loginRequest = new LoginRequest
        {
            Email = "admin@demo.com",
            Password = "Admin123!"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<LoginResponse>(loginContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        var response = await _client.GetAsync("/api/forms");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthenticatedRequest_InvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        var response = await _client.GetAsync("/api/forms");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_Always_ReturnsSuccess()
    {
        var response = await _client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}