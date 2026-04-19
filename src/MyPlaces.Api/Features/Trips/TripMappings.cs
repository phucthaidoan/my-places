using MyPlaces.Api.Common.Entities;
using MyPlaces.Api.Features.Places;
using MyPlaces.Shared.Trips;
using SharedTripVisibility = MyPlaces.Shared.Trips.TripVisibility;
using E = MyPlaces.Api.Common.Entities;

namespace MyPlaces.Api.Features.Trips;

internal static class TripMappings
{
    internal static SharedTripVisibility ToShared(E.TripVisibility v) => v switch
    {
        E.TripVisibility.Private => SharedTripVisibility.Private,
        E.TripVisibility.PublicLink => SharedTripVisibility.PublicLink,
        _ => throw new ArgumentOutOfRangeException(nameof(v))
    };

    internal static E.TripVisibility ToEntity(SharedTripVisibility v) => v switch
    {
        SharedTripVisibility.Private => E.TripVisibility.Private,
        SharedTripVisibility.PublicLink => E.TripVisibility.PublicLink,
        _ => throw new ArgumentOutOfRangeException(nameof(v))
    };

    internal static TripSummaryResponse ToSummary(Trip trip) => new(
        trip.Id,
        trip.Name,
        trip.City,
        trip.StartDate,
        trip.EndDate,
        ToShared(trip.Visibility),
        trip.TripPlaces.Count,
        trip.CreatedAt
    );

    internal static TripDetailResponse ToDetail(Trip trip)
    {
        var ordered = trip.TripPlaces
            .OrderBy(tp => tp.AddedAt)
            .Select(tp => new TripPlaceResponse(tp.AddedAt, CreatePlace.ToResponse(tp.Place)))
            .ToList();

        return new TripDetailResponse(
            trip.Id,
            trip.Name,
            trip.City,
            trip.Country,
            trip.StartDate,
            trip.EndDate,
            ToShared(trip.Visibility),
            trip.CreatedAt,
            ordered
        );
    }
}
