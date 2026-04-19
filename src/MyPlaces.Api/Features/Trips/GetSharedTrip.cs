using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;
using E = MyPlaces.Api.Common.Entities;

namespace MyPlaces.Api.Features.Trips;

public static class GetSharedTrip
{
    public static async Task<IResult> Handle(
        string token,
        AppDbContext db,
        CancellationToken ct)
    {
        var trip = await db.Trips
            .AsNoTracking()
            .Include(t => t.TripPlaces)
            .ThenInclude(tp => tp.Place)
            .ThenInclude(p => p.Photos)
            .FirstOrDefaultAsync(
                t => t.ShareToken == token && t.Visibility == E.TripVisibility.PublicLink,
                ct);

        if (trip is null)
            return Results.NotFound();

        return Results.Ok(Result<TripDetailResponse>.Success(TripMappings.ToDetail(trip)));
    }
}
