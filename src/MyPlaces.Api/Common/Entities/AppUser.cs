using Microsoft.AspNetCore.Identity;

namespace MyPlaces.Api.Common.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string Username { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Place> Places { get; set; } = [];
    public ICollection<Trip> Trips { get; set; } = [];
    public ICollection<Follow> Followers { get; set; } = [];
    public ICollection<Follow> Following { get; set; } = [];
}
