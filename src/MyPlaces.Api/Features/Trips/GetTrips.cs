using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Features.Trips;

public static class GetTrips
{
    public static async Task<IResult> Handle(
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var trips = await db.Trips
            .Include(t => t.TripPlaces)
            .Where(t => t.UserId == currentUser.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);

        var list = trips.Select(TripMappings.ToSummary).ToList();
        return Results.Ok(Result<List<TripSummaryResponse>>.Success(list));
    }
}
