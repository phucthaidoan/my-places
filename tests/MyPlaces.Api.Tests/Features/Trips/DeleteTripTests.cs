using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Tests.Features.Trips;

public class DeleteTripTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task DeleteTrip_Owner_Returns204_AndSubsequentGet404()
    {
        await RegisterAndLoginAsync(email: "trip-del1@test.com", username: "trip-del1");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("N", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        var del = await Client.DeleteAsync($"/api/v1/trips/{tripId}");
        del.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var get = await Client.GetAsync($"/api/v1/trips/{tripId}");
        get.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTrip_OtherUser_Returns404()
    {
        await RegisterAndLoginAsync(email: "trip-del2a@test.com", username: "trip-del2a");
        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("N", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        Client.DefaultRequestHeaders.Clear();
        await RegisterAndLoginAsync(email: "trip-del2b@test.com", username: "trip-del2b");

        var del = await Client.DeleteAsync($"/api/v1/trips/{tripId}");
        del.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
