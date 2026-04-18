using MyPlaces.Api.Common;
using MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Features.Places;

public static class CreatePlace
{
    public static async Task<IResult> Handle(
        CreatePlaceRequest req,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var place = new Place
        {
            UserId = currentUser.Id,
            Name = req.Name,
            Address = req.Address,
            City = req.City,
            Country = req.Country,
            Latitude = req.Latitude,
            Longitude = req.Longitude,
            Note = req.Note
        };

        db.Places.Add(place);
        await db.SaveChangesAsync(ct);

        return Results.Ok(Result<PlaceResponse>.Success(ToResponse(place)));
    }

    internal static PlaceResponse ToResponse(Place place) => new(
        Id: place.Id,
        Name: place.Name,
        Address: place.Address,
        City: place.City,
        Country: place.Country,
        Latitude: place.Latitude,
        Longitude: place.Longitude,
        Note: place.Note,
        SourcePlaceId: place.SourcePlaceId,
        CreatedAt: place.CreatedAt,
        Photos: place.Photos.Select(p => new PlacePhotoResponse(
            p.Id, p.PhotoUrl, p.IsExternal, p.SortOrder)).ToList()
    );
}
