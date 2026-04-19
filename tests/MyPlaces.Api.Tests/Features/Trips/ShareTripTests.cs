using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Tests.Features.Trips;

public class ShareTripTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task ShareTrip_PrivateToPublic_YieldsToken_PublicToPrivate_Clears()
    {
        await RegisterAndLoginAsync(email: "trip-sh1@test.com", username: "trip-sh1");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        var pub = await Client.PostAsync($"/api/v1/trips/{tripId}/share", null);
        pub.EnsureSuccessStatusCode();
        var pubBody = await pub.Content.ReadFromJsonAsync<Result<ShareTripResponse>>(ApiJson);
        pubBody!.Value!.Visibility.Should().Be(TripVisibility.PublicLink);
        pubBody.Value.ShareToken.Should().NotBeNullOrEmpty();
        pubBody.Value.SharePath.Should().StartWith("/shared/");

        var priv = await Client.PostAsync($"/api/v1/trips/{tripId}/share", null);
        priv.EnsureSuccessStatusCode();
        var privBody = await priv.Content.ReadFromJsonAsync<Result<ShareTripResponse>>(ApiJson);
        privBody!.Value!.Visibility.Should().Be(TripVisibility.Private);
        privBody.Value.ShareToken.Should().BeNull();
        privBody.Value.SharePath.Should().BeNull();
    }

    [Fact]
    public async Task ShareTrip_SecondPublicToggle_GeneratesNewToken()
    {
        await RegisterAndLoginAsync(email: "trip-sh2@test.com", username: "trip-sh2");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("T", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        var first = await Client.PostAsync($"/api/v1/trips/{tripId}/share", null);
        var token1 = (await first.Content.ReadFromJsonAsync<Result<ShareTripResponse>>(ApiJson))!.Value!.ShareToken;

        await Client.PostAsync($"/api/v1/trips/{tripId}/share", null);
        var third = await Client.PostAsync($"/api/v1/trips/{tripId}/share", null);
        var token2 = (await third.Content.ReadFromJsonAsync<Result<ShareTripResponse>>(ApiJson))!.Value!.ShareToken;

        token2.Should().NotBe(token1);
    }

    [Fact]
    public async Task ShareTrip_NotFound_Returns404()
    {
        await RegisterAndLoginAsync(email: "trip-sh3@test.com", username: "trip-sh3");
        var response = await Client.PostAsync($"/api/v1/trips/{Guid.NewGuid()}/share", null);
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
