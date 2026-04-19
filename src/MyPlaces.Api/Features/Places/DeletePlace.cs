using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common;

namespace MyPlaces.Api.Features.Places;

public static class DeletePlace
{
    public static async Task<IResult> Handle(
        Guid id,
        AppDbContext db,
        ICurrentUser currentUser,
        CancellationToken ct)
    {
        var place = await db.Places
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUser.Id, ct);

        if (place is null)
            return Results.NotFound();

        db.Places.Remove(place);
        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}
