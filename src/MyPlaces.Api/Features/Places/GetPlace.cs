using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Features.Places;

public static class GetPlace
{
    public static async Task<IResult> Handle(
        Guid id,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var place = await db.Places
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUser.Id, ct);

        if (place is null)
            return Results.NotFound();

        return Results.Ok(Result<PlaceResponse>.Success(CreatePlace.ToResponse(place)));
    }
}
