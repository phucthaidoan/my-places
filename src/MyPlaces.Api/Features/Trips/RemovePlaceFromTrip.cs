using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Features.Trips;

public static class RemovePlaceFromTrip
{
    public static async Task<IResult> Handle(
        Guid id,
        Guid placeId,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var trip = await db.Trips
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUser.Id, ct);

        if (trip is null)
            return Results.NotFound();

        var link = await db.TripPlaces
            .FirstOrDefaultAsync(tp => tp.TripId == id && tp.PlaceId == placeId, ct);

        if (link is null)
            return Results.NotFound();

        db.TripPlaces.Remove(link);
        await db.SaveChangesAsync(ct);

        await db.Entry(trip).Collection(t => t.TripPlaces).Query()
            .Include(tp => tp.Place)
            .ThenInclude(p => p.Photos)
            .LoadAsync(ct);

        return Results.Ok(Result<TripDetailResponse>.Success(TripMappings.ToDetail(trip)));
    }
}
