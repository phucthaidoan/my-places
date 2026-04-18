namespace MyPlaces.Api.Common.Entities;

public class TripPlace
{
    public Guid TripId { get; set; }
    public Guid PlaceId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public Trip Trip { get; set; } = default!;
    public Place Place { get; set; } = default!;
}
