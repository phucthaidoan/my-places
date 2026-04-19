using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Features.Trips;

public static class GetTrip
{
    public static async Task<IResult> Handle(
        Guid id,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var trip = await db.Trips
            .Include(t => t.TripPlaces)
            .ThenInclude(tp => tp.Place)
            .ThenInclude(p => p.Photos)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == currentUser.Id, ct);

        if (trip is null)
            return Results.NotFound();

        return Results.Ok(Result<TripDetailResponse>.Success(TripMappings.ToDetail(trip)));
    }
}
