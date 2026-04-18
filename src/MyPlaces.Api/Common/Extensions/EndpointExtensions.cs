using MyPlaces.Api.Features.Auth;

namespace MyPlaces.Api.Common.Extensions;

public static class EndpointExtensions
{
    public static WebApplication MapAllEndpoints(this WebApplication app)
    {
        var v1 = app.MapGroup("/api/v1");

        // Auth
        v1.MapPost("/auth/register", Register.Handle);
        v1.MapPost("/auth/login", Login.Handle);
        v1.MapPost("/auth/google", GoogleLogin.Handle);
        v1.MapPost("/auth/refresh-token", RefreshToken.Handle);

        return app;
    }
}
