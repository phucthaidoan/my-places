using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyPlaces.Api.Common;
using Entities = MyPlaces.Api.Common.Entities;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Trips;

namespace MyPlaces.Api.Tests.Features.Trips;

public class UpdateTripTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task UpdateTrip_Owner_UpdatesFields()
    {
        await RegisterAndLoginAsync(email: "trip-up1@test.com", username: "trip-up1");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("N", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        var put = new UpdateTripRequest(
            Name: "New",
            City: "HCMC",
            Country: "VN",
            StartDate: null,
            EndDate: null,
            Visibility: TripVisibility.Private);

        var response = await Client.PutAsJsonAsync($"/api/v1/trips/{tripId}", put);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson);
        result!.Value!.Name.Should().Be("New");
        result.Value.City.Should().Be("HCMC");
    }

    [Fact]
    public async Task UpdateTrip_SetPrivate_ClearsShareToken()
    {
        await RegisterAndLoginAsync(email: "trip-up2@test.com", username: "trip-up2");

        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips",
                new CreateTripRequest("N", "C", null, null, null, TripVisibility.PublicLink)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        var put = new UpdateTripRequest("N", "C", null, null, null, TripVisibility.Private);
        await Client.PutAsJsonAsync($"/api/v1/trips/{tripId}", put);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var trip = await db.Trips.SingleAsync(t => t.Id == tripId);
        trip.ShareToken.Should().BeNull();
        trip.Visibility.Should().Be(Entities.TripVisibility.Private);
    }

    [Fact]
    public async Task UpdateTrip_NotFound_Returns404()
    {
        await RegisterAndLoginAsync(email: "trip-up3@test.com", username: "trip-up3");
        var put = new UpdateTripRequest("N", "C", null, null, null, TripVisibility.Private);
        var response = await Client.PutAsJsonAsync($"/api/v1/trips/{Guid.NewGuid()}", put);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTrip_OtherUser_Returns404()
    {
        await RegisterAndLoginAsync(email: "trip-up4a@test.com", username: "trip-up4a");
        var tripId = (await (await Client.PostAsJsonAsync("/api/v1/trips", new CreateTripRequest("N", "C", null, null, null)))
            .Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson))!.Value!.Id;

        Client.DefaultRequestHeaders.Clear();
        await RegisterAndLoginAsync(email: "trip-up4b@test.com", username: "trip-up4b");

        var put = new UpdateTripRequest("H", "C", null, null, null, TripVisibility.Private);
        var response = await Client.PutAsJsonAsync($"/api/v1/trips/{tripId}", put);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
