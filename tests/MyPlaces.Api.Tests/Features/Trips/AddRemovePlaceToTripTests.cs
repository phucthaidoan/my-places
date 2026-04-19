using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Tests.Features.Trips;

public class AddRemovePlaceToTripTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task AddPlaceTwice_SecondReturns409()
    {
        await RegisterAndLoginAsync(email: "trip-ap1@test.com", username: "trip-ap1");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;
        var placeId = (await (await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest("P", "A", "C", null, null, null, null)))
            .Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        var first = await Client.PostAsync($"/api/v1/trips/{tripId}/places/{placeId}", null);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await Client.PostAsync($"/api/v1/trips/{tripId}/places/{placeId}", null);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AddPlace_OtherUsersPlace_Returns404()
    {
        await RegisterAndLoginAsync(email: "trip-ap2a@test.com", username: "trip-ap2a");
        var placeId = (await (await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest("P", "A", "C", null, null, null, null)))
            .Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        Client.DefaultRequestHeaders.Clear();
        await RegisterAndLoginAsync(email: "trip-ap2b@test.com", username: "trip-ap2b");
        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        var response = await Client.PostAsync($"/api/v1/trips/{tripId}/places/{placeId}", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemovePlace_RemovesMembershipOnly()
    {
        await RegisterAndLoginAsync(email: "trip-ap3@test.com", username: "trip-ap3");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;
        var placeId = (await (await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest("P", "A", "C", null, null, null, null)))
            .Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        await Client.PostAsync($"/api/v1/trips/{tripId}/places/{placeId}", null);

        var del = await Client.DeleteAsync($"/api/v1/trips/{tripId}/places/{placeId}");
        del.StatusCode.Should().Be(HttpStatusCode.OK);

        var getPlace = await Client.GetAsync($"/api/v1/places/{placeId}");
        getPlace.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemovePlace_NotMember_Returns404()
    {
        await RegisterAndLoginAsync(email: "trip-ap4@test.com", username: "trip-ap4");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;
        var placeId = (await (await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest("P", "A", "C", null, null, null, null)))
            .Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        var del = await Client.DeleteAsync($"/api/v1/trips/{tripId}/places/{placeId}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
