using System.ComponentModel.DataAnnotations;

namespace MyPlaces.Shared.Places;

public record CreatePlaceRequest(
    [Required] string Name,
    [Required] string Address,
    [Required] string City,
    string? Country,
    double? Latitude,
    double? Longitude,
    string? Note
);

public record UpdatePlaceRequest(
    [Required] string Name,
    [Required] string Address,
    [Required] string City,
    string? Country,
    double? Latitude,
    double? Longitude,
    string? Note
);
