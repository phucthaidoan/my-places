using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;
using E = MyPlaces.Api.Common.Entities;

namespace MyPlaces.Api.Features.Trips;

public static class UpdateTrip
{
    public static async Task<IResult> Handle(
        Guid id,
        UpdateTripRequest req,
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

        trip.Name = req.Name;
        trip.City = req.City;
        trip.Country = req.Country;
        trip.StartDate = req.StartDate;
        trip.EndDate = req.EndDate;

        var newVisibility = TripMappings.ToEntity(req.Visibility);
        await ApplyVisibilityAsync(trip, newVisibility, db, ct);

        await db.SaveChangesAsync(ct);

        return Results.Ok(Result<TripDetailResponse>.Success(TripMappings.ToDetail(trip)));
    }

    internal static async Task ApplyVisibilityAsync(
        Trip trip,
        E.TripVisibility newVisibility,
        AppDbContext db,
        CancellationToken ct)
    {
        if (newVisibility == E.TripVisibility.Private)
        {
            trip.Visibility = E.TripVisibility.Private;
            trip.ShareToken = null;
            return;
        }

        trip.Visibility = E.TripVisibility.PublicLink;
        if (string.IsNullOrEmpty(trip.ShareToken))
            trip.ShareToken = await TripShareToken.GenerateUniqueAsync(db, ct);
    }
}
