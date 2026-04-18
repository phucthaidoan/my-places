using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Tests.Features.Auth;

public class RegisterTests(TestWebAppFactory factory)
    : IClassFixture<TestWebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ReturnsToken()
    {
        var request = new RegisterRequest(
            Email: "test@example.com",
            Username: "testuser",
            Password: "Password1",
            DisplayName: "Test User"
        );

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().NotBeEmpty();
        result.Value.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
    {
        var request = new RegisterRequest(
            Email: "dup@example.com",
            Username: "dupuser",
            Password: "Password1",
            DisplayName: "Dup User"
        );

        await _client.PostAsJsonAsync("/api/v1/auth/register", request);
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
