namespace MyPlaces.Api.Common.Entities;

public class Follow
{
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public AppUser Follower { get; set; } = default!;
    public AppUser FollowingNav { get; set; } = default!;
}
