namespace MyPlaces.Api.Common.Entities;

public enum TripVisibility { Private, PublicLink }

public class Trip
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = default!;
    public string City { get; set; } = default!;
    public string? Country { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TripVisibility Visibility { get; set; } = TripVisibility.Private;
    public string? ShareToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser User { get; set; } = default!;
    public ICollection<TripPlace> TripPlaces { get; set; } = [];
}
