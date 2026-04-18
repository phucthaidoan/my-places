using System.Net.Http.Headers;
using System.Net.Http.Json;
using MyPlaces.Shared.Auth;
using MyPlaces.Shared.Common;

namespace MyPlaces.Api.Tests;

public abstract class AuthenticatedTestBase : IClassFixture<TestWebAppFactory>
{
    protected readonly HttpClient Client;

    protected AuthenticatedTestBase(TestWebAppFactory factory)
    {
        Client = factory.CreateClient();
    }

    protected async Task<string> RegisterAndLoginAsync(
        string email = "user@test.com",
        string username = "testuser",
        string password = "Password1",
        string displayName = "Test User")
    {
        await Client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest(
            Email: email, Username: username, Password: password, DisplayName: displayName));

        var loginResp = await Client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest(Email: email, Password: password));

        var result = await loginResp.Content.ReadFromJsonAsync<Result<AuthResponse>>();
        var token = result!.Value!.Token;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }
}
