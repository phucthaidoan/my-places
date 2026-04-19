using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Features.Places;

public static class CopyPlace
{
    public static async Task<IResult> Handle(
        Guid id,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        // Intentional: any authenticated user can copy any place (social feature).
        // The copy is owned by currentUser — source place data is read-only.
        var source = await db.Places
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (source is null)
            return Results.NotFound();

        var copy = new Place
        {
            UserId = currentUser.Id,
            Name = source.Name,
            Address = source.Address,
            City = source.City,
            Country = source.Country,
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            Note = source.Note,
            SourcePlaceId = source.Id
        };

        db.Places.Add(copy);
        await db.SaveChangesAsync(ct);

        return Results.Ok(Result<PlaceResponse>.Success(CreatePlace.ToResponse(copy)));
    }
}
