using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Features.Places;

public static class GetPlaces
{
    public static async Task<IResult> Handle(
        AppDbContext db,
        ICurrentUser currentUser,
        string? q,
        string? city,
        CancellationToken ct)
    {
        var query = db.Places
            .Include(p => p.Photos)
            .Where(p => p.UserId == currentUser.Id);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(p => EF.Functions.ILike(p.City, $"%{city}%"));

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, $"%{q}%") ||
                EF.Functions.ILike(p.Address, $"%{q}%") ||
                (p.Note != null && EF.Functions.ILike(p.Note, $"%{q}%")));

        var places = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PlaceSummaryResponse(
                p.Id,
                p.Name,
                p.City,
                p.Country,
                p.Latitude,
                p.Longitude,
                p.CreatedAt,
                p.Photos.OrderBy(ph => ph.SortOrder).Select(ph => ph.PhotoUrl).FirstOrDefault()
            ))
            .ToListAsync(ct);

        return Results.Ok(Result<List<PlaceSummaryResponse>>.Success(places));
    }
}
