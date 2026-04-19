using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Tests.Features.Trips;

public class GetSharedTripTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task GetSharedTrip_PublicLink_ReturnsTripWithoutAuth()
    {
        await RegisterAndLoginAsync(email: "trip-gsh1@test.com", username: "trip-gsh1");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        var share = await Client.PostAsync($"/api/v1/trips/{tripId}/share", null);
        var token = (await share.Content.ReadFromJsonAsync<Result<ShareTripResponse>>(ApiJson))!.Value!.ShareToken!;

        var anon = Factory.CreateClient();
        var response = await anon.GetAsync($"/api/v1/trips/shared/{token}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson);
        result!.Value!.Name.Should().Be("T");
    }

    [Fact]
    public async Task GetSharedTrip_BadToken_Returns404()
    {
        var anon = Factory.CreateClient();
        var response = await anon.GetAsync("/api/v1/trips/shared/invalid-token-xxxxxxxx");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetSharedTrip_WhenPrivate_Returns404()
    {
        await RegisterAndLoginAsync(email: "trip-gsh2@test.com", username: "trip-gsh2");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        var share = await Client.PostAsync($"/api/v1/trips/{tripId}/share", null);
        var token = (await share.Content.ReadFromJsonAsync<Result<ShareTripResponse>>(ApiJson))!.Value!.ShareToken!;

        await Client.PostAsync($"/api/v1/trips/{tripId}/share", null);

        var anon = Factory.CreateClient();
        var response = await anon.GetAsync($"/api/v1/trips/shared/{token}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
