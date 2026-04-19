using System.ComponentModel.DataAnnotations;

namespace MyPlaces.Shared.Trips;

public record CreateTripRequest(
    [Required] string Name,
    [Required] string City,
    string? Country,
    DateTime? StartDate,
    DateTime? EndDate,
    TripVisibility Visibility = TripVisibility.Private
);

public record UpdateTripRequest(
    [Required] string Name,
    [Required] string City,
    string? Country,
    DateTime? StartDate,
    DateTime? EndDate,
    TripVisibility Visibility
);
