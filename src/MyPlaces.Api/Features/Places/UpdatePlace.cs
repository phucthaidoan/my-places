using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Features.Places;

public static class UpdatePlace
{
    public static async Task<IResult> Handle(
        Guid id,
        UpdatePlaceRequest req,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var place = await db.Places
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUser.Id, ct);

        if (place is null)
            return Results.NotFound();

        place.Name = req.Name;
        place.Address = req.Address;
        place.City = req.City;
        place.Country = req.Country;
        place.Latitude = req.Latitude;
        place.Longitude = req.Longitude;
        place.Note = req.Note;

        await db.SaveChangesAsync(ct);

        return Results.Ok(Result<PlaceResponse>.Success(CreatePlace.ToResponse(place)));
    }
}
