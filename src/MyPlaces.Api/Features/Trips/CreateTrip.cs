using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;
using E = MyPlaces.Api.Common.Entities;

namespace MyPlaces.Api.Features.Trips;

public static class CreateTrip
{
    public static async Task<IResult> Handle(
        CreateTripRequest req,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        string? shareToken = null;
        var visibility = TripMappings.ToEntity(req.Visibility);
        if (visibility == E.TripVisibility.PublicLink)
            shareToken = await TripShareToken.GenerateUniqueAsync(db, ct);

        var trip = new Trip
        {
            UserId = currentUser.Id,
            Name = req.Name,
            City = req.City,
            Country = req.Country,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            Visibility = visibility,
            ShareToken = shareToken
        };

        db.Trips.Add(trip);
        await db.SaveChangesAsync(ct);

        await db.Entry(trip).Collection(t => t.TripPlaces).LoadAsync(ct);
        return Results.Ok(Result<TripDetailResponse>.Success(TripMappings.ToDetail(trip)));
    }
}
