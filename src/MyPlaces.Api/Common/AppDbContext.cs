using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyPlaces.Api.Common.Entities;

namespace MyPlaces.Api.Common;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Place> Places => Set<Place>();
    public DbSet<PlacePhoto> PlacePhotos => Set<PlacePhoto>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripPlace> TripPlaces => Set<TripPlace>();
    public DbSet<Follow> Follows => Set<Follow>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TripPlace>()
            .HasKey(tp => new { tp.TripId, tp.PlaceId });

        builder.Entity<Follow>()
            .HasKey(f => new { f.FollowerId, f.FollowingId });

        builder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Following)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Follow>()
            .HasOne(f => f.FollowingNav)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.Entity<Place>()
            .HasIndex(p => new { p.UserId, p.CreatedAt });
        builder.Entity<Place>()
            .HasIndex(p => new { p.City, p.UserId });
        builder.Entity<Trip>()
            .HasIndex(t => new { t.UserId, t.CreatedAt });
        builder.Entity<Trip>()
            .HasIndex(t => t.ShareToken)
            .IsUnique()
            .HasFilter("share_token IS NOT NULL");
        builder.Entity<Follow>()
            .HasIndex(f => new { f.FollowerId, f.FollowingId });
        builder.Entity<PlacePhoto>()
            .HasIndex(pp => new { pp.PlaceId, pp.SortOrder });
    }
}
