namespace MyPlaces.Shared.Places;

public record CreatePlaceRequest(
    string Name,
    string Address,
    string City,
    string? Country,
    double? Latitude,
    double? Longitude,
    string? Note
);

public record UpdatePlaceRequest(
    string Name,
    string Address,
    string City,
    string? Country,
    double? Latitude,
    double? Longitude,
    string? Note
);
