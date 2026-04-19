using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Tests.Features.Trips;

public class GetTripTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task GetTrip_Owner_ReturnsDetailWithPlacesOrdered()
    {
        await RegisterAndLoginAsync(email: "trip-get1@test.com", username: "trip-get1");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        var p1 = (await (await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest("P1", "A", "C", null, null, null, null)))
            .Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;
        var p2 = (await (await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest("P2", "A", "C", null, null, null, null)))
            .Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        await Client.PostAsync($"/api/v1/trips/{tripId}/places/{p1}", null);
        await Task.Delay(5);
        await Client.PostAsync($"/api/v1/trips/{tripId}/places/{p2}", null);

        var response = await Client.GetAsync($"/api/v1/trips/{tripId}");
        var result = await response.Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson);

        result!.Value!.Places.Should().HaveCount(2);
        result.Value.Places[0].Place.Name.Should().Be("P1");
        result.Value.Places[1].Place.Name.Should().Be("P2");
    }

    [Fact]
    public async Task GetTrip_UnknownId_Returns404()
    {
        await RegisterAndLoginAsync(email: "trip-get2@test.com", username: "trip-get2");
        var response = await Client.GetAsync($"/api/v1/trips/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTrip_OtherUsersTrip_Returns404()
    {
        await RegisterAndLoginAsync(email: "trip-get3a@test.com", username: "trip-get3a");
        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        Client.DefaultRequestHeaders.Clear();
        await RegisterAndLoginAsync(email: "trip-get3b@test.com", username: "trip-get3b");

        var response = await Client.GetAsync($"/api/v1/trips/{tripId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
