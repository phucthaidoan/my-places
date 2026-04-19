using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Tests.Features.Trips;

public class GetTripsTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task GetTrips_ReturnsOnlyOwnTrips_OrderedByCreatedDesc()
    {
        await RegisterAndLoginAsync(email: "trip-list1@test.com", username: "trip-list1");

        await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("A", "City", null, null, null));
        await Task.Delay(5);
        await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("B", "City", null, null, null));

        var response = await Client.GetAsync("/api/v1/trips");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<List<TripSummaryResponse>>>(ApiJson);
        result!.Value!.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("B");
        result.Value[1].Name.Should().Be("A");
    }

    [Fact]
    public async Task GetTrips_IncludesPlaceCount()
    {
        await RegisterAndLoginAsync(email: "trip-list2@test.com", username: "trip-list2");

        var tripResp = await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null));
        var tripId = (await tripResp.Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        var placeResp = await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "P", "Addr", "C", null, null, null, null));
        var placeId = (await placeResp.Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        await Client.PostAsync($"/api/v1/trips/{tripId}/places/{placeId}", null);

        var listResp = await Client.GetAsync("/api/v1/trips");
        var list = await listResp.Content.ReadFromJsonAsync<Result<List<TripSummaryResponse>>>(ApiJson);
        list!.Value!.Single().PlaceCount.Should().Be(1);
    }

    [Fact]
    public async Task GetTrips_Unauthenticated_Returns401()
    {
        Client.DefaultRequestHeaders.Clear();
        var response = await Client.GetAsync("/api/v1/trips");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
