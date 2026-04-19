namespace MyPlaces.Shared.Places;

public record PlacePhotoResponse(
    Guid Id,
    string PhotoUrl,
    bool IsExternal,
    int SortOrder
);

public record PlaceResponse(
    Guid Id,
    string Name,
    string Address,
    string City,
    string? Country,
    double? Latitude,
    double? Longitude,
    string? Note,
    Guid? SourcePlaceId,
    DateTime CreatedAt,
    List<PlacePhotoResponse> Photos
);

public record PlaceSummaryResponse(
    Guid Id,
    string Name,
    string City,
    string? Country,
    double? Latitude,
    double? Longitude,
    DateTime CreatedAt,
    string? FirstPhotoUrl
);
