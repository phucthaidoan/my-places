using MyPlaces.Api.Features.Auth;
using MyPlaces.Api.Features.Places;

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

        // Places — /places/nearby must be before /places/{id:guid}
        v1.MapPost("/places", CreatePlace.Handle).RequireAuthorization();
        v1.MapGet("/places", GetPlaces.Handle).RequireAuthorization();
        v1.MapGet("/places/nearby", GetNearbyPlaces.Handle).RequireAuthorization();
        v1.MapGet("/places/{id:guid}", GetPlace.Handle).RequireAuthorization();
        v1.MapPut("/places/{id:guid}", UpdatePlace.Handle).RequireAuthorization();
        v1.MapDelete("/places/{id:guid}", DeletePlace.Handle).RequireAuthorization();
        v1.MapPost("/places/{id:guid}/copy", CopyPlace.Handle).RequireAuthorization();

        return app;
    }
}
