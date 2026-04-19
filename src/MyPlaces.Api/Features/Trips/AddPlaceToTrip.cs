using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Features.Trips;

public static class AddPlaceToTrip
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

        var place = await db.Places
            .FirstOrDefaultAsync(p => p.Id == placeId && p.UserId == currentUser.Id, ct);

        if (place is null)
            return Results.NotFound();

        var exists = await db.TripPlaces.AnyAsync(tp => tp.TripId == id && tp.PlaceId == placeId, ct);
        if (exists)
            return Results.Conflict();

        db.TripPlaces.Add(new()
        {
            TripId = id,
            PlaceId = placeId,
            AddedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);

        await db.Entry(trip).Collection(t => t.TripPlaces).Query()
            .Include(tp => tp.Place)
            .ThenInclude(p => p.Photos)
            .LoadAsync(ct);

        return Results.Ok(Result<TripDetailResponse>.Success(TripMappings.ToDetail(trip)));
    }
}
