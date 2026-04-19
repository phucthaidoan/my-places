using MyPlaces.Api.Features.Auth;
using MyPlaces.Api.Features.Places;
using MyPlaces.Api.Features.Trips;

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

        // Places — /places/nearby before /places/{id:guid}
        v1.MapPost("/places", CreatePlace.Handle).RequireAuthorization();
        v1.MapGet("/places", GetPlaces.Handle).RequireAuthorization();
        v1.MapGet("/places/nearby", GetNearbyPlaces.Handle).RequireAuthorization();
        v1.MapGet("/places/{id:guid}", GetPlace.Handle).RequireAuthorization();
        v1.MapPut("/places/{id:guid}", UpdatePlace.Handle).RequireAuthorization();
        v1.MapDelete("/places/{id:guid}", DeletePlace.Handle).RequireAuthorization();
        v1.MapPost("/places/{id:guid}/copy", CopyPlace.Handle).RequireAuthorization();

        // Trips — shared route before /trips/{id:guid}
        v1.MapGet("/trips/shared/{token}", GetSharedTrip.Handle);
        v1.MapGet("/trips", GetTrips.Handle).RequireAuthorization();
        v1.MapPost("/trips", CreateTrip.Handle).RequireAuthorization();
        v1.MapGet("/trips/{id:guid}", GetTrip.Handle).RequireAuthorization();
        v1.MapPut("/trips/{id:guid}", UpdateTrip.Handle).RequireAuthorization();
        v1.MapDelete("/trips/{id:guid}", DeleteTrip.Handle).RequireAuthorization();
        v1.MapPost("/trips/{id:guid}/places/{placeId:guid}", AddPlaceToTrip.Handle).RequireAuthorization();
        v1.MapDelete("/trips/{id:guid}/places/{placeId:guid}", RemovePlaceFromTrip.Handle).RequireAuthorization();
        v1.MapPost("/trips/{id:guid}/share", ShareTrip.Handle).RequireAuthorization();

        return app;
    }
}
