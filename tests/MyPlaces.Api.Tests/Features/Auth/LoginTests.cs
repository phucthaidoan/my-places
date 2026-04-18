using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Tests.Features.Auth;

public class LoginTests(TestWebAppFactory factory)
    : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(
            Email: "login@example.com",
            Username: "loginuser",
            Password: "Password1",
            DisplayName: "Login User"
        ));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(Email: "login@example.com", Password: "Password1"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(
            Email: "wrong@example.com",
            Username: "wronguser",
            Password: "Password1",
            DisplayName: "Wrong User"
        ));

        var response = await _client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(Email: "wrong@example.com", Password: "WrongPassword1"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
