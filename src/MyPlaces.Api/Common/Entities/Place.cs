namespace MyPlaces.Api.Common.Entities;

public class Place
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Note { get; set; }
    public Guid? SourcePlaceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = default!;
    public ICollection<PlacePhoto> Photos { get; set; } = [];
    public ICollection<TripPlace> TripPlaces { get; set; } = [];
}
