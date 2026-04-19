using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;

namespace MyPlaces.Api.Features.Trips;

public static class DeleteTrip
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

        db.Trips.Remove(trip);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
