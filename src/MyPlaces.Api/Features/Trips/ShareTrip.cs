using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;
using E = MyPlaces.Api.Common.Entities;

namespace MyPlaces.Api.Features.Trips;

public static class ShareTrip
{
    public static async Task<IResult> Handle(
        Guid id,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var trip = await db.Trips
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUser.Id, ct);

        if (trip is null)
            return Results.NotFound();

        if (trip.Visibility == E.TripVisibility.Private)
        {
            trip.Visibility = E.TripVisibility.PublicLink;
            trip.ShareToken = await TripShareToken.GenerateUniqueAsync(db, ct);
        }
        else
        {
            trip.Visibility = E.TripVisibility.Private;
            trip.ShareToken = null;
        }

        await db.SaveChangesAsync(ct);

        var sharePath = trip.Visibility == E.TripVisibility.PublicLink && trip.ShareToken is not null
            ? $"/shared/{trip.ShareToken}"
            : null;

        var body = new ShareTripResponse(
            TripMappings.ToShared(trip.Visibility),
            trip.ShareToken,
            sharePath);

        return Results.Ok(Result<ShareTripResponse>.Success(body));
    }
}
