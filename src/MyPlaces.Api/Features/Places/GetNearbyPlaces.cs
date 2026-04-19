using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Features.Places;

public static class GetNearbyPlaces
{
    private const double EarthRadiusKm = 6371.0;

    public static async Task<IResult> Handle(
        AppDbContext db,
        ICurrentUser currentUser,
        double? lat,
        double? lng,
        double radiusKm = 5,
        CancellationToken ct = default)
    {
        if (lat is null || lng is null)
            return Results.BadRequest(Result<List<PlaceSummaryResponse>>.Failure("lat and lng are required"));

        if (radiusKm <= 0 || radiusKm > 500)
            return Results.BadRequest(Result<List<PlaceSummaryResponse>>.Failure("radiusKm must be between 0 and 500"));

        var candidates = await db.Places
            .Include(p => p.Photos)
            .Where(p => p.UserId == currentUser.Id && p.Latitude != null && p.Longitude != null)
            .ToListAsync(ct);

        var nearby = candidates
            .Select(p => new
            {
                Place = p,
                DistanceKm = HaversineKm(lat.Value, lng.Value, p.Latitude!.Value, p.Longitude!.Value)
            })
            .Where(x => x.DistanceKm <= radiusKm)
            .OrderBy(x => x.DistanceKm)
            .Select(x => new PlaceSummaryResponse(
                x.Place.Id,
                x.Place.Name,
                x.Place.City,
                x.Place.Country,
                x.Place.Latitude,
                x.Place.Longitude,
                x.Place.CreatedAt,
                x.Place.Photos.OrderBy(ph => ph.SortOrder).Select(ph => ph.PhotoUrl).FirstOrDefault()
            ))
            .ToList();

        return Results.Ok(Result<List<PlaceSummaryResponse>>.Success(nearby));
    }

    private static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180.0;
}
