using MyPlaces.Shared.Places;

namespace MyPlaces.Shared.Trips;

public record TripSummaryResponse(
    Guid Id,
    string Name,
    string City,
    DateTime? StartDate,
    DateTime? EndDate,
    TripVisibility Visibility,
    int PlaceCount,
    DateTime CreatedAt
);

public record TripPlaceResponse(DateTime AddedAt, PlaceResponse Place);

public record TripDetailResponse(
    Guid Id,
    string Name,
    string City,
    string? Country,
    DateTime? StartDate,
    DateTime? EndDate,
    TripVisibility Visibility,
    DateTime CreatedAt,
    IReadOnlyList<TripPlaceResponse> Places
);

public record ShareTripResponse(
    TripVisibility Visibility,
    string? ShareToken,
    string? SharePath
);
