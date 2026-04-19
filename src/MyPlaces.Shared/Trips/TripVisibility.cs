using System.Text.Json.Serialization;

namespace MyPlaces.Shared.Trips;

[JsonConverter(typeof(JsonStringEnumConverter<TripVisibility>))]
public enum TripVisibility
{
    Private,
    PublicLink
}
