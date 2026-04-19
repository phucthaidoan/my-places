using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Tests.Features.Places;

public class GetNearbyPlacesTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task GetNearbyPlaces_ReturnsPlacesWithinRadius()
    {
        await RegisterAndLoginAsync(email: "nearby1@test.com", username: "nearby1");

        await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Near Place", "1 St", "Hanoi", "Vietnam", 21.030, 105.835, null));
        await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Far Place", "2 St", "HCMC", "Vietnam", 10.776, 106.700, null));

        var response = await Client.GetAsync("/api/v1/places/nearby?lat=21.028&lng=105.834&radiusKm=5");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<List<PlaceSummaryResponse>>>();
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Near Place");
    }

    [Fact]
    public async Task GetNearbyPlaces_NoCoordinates_ReturnsEmptyList()
    {
        await RegisterAndLoginAsync(email: "nearby2@test.com", username: "nearby2");

        await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "No GPS", "1 St", "Hanoi", null, null, null, null));

        var response = await Client.GetAsync("/api/v1/places/nearby?lat=21.028&lng=105.834&radiusKm=5");

        var result = await response.Content.ReadFromJsonAsync<Result<List<PlaceSummaryResponse>>>();
        result!.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetNearbyPlaces_MissingParams_ReturnsBadRequest()
    {
        await RegisterAndLoginAsync(email: "nearby3@test.com", username: "nearby3");

        var response = await Client.GetAsync("/api/v1/places/nearby");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
