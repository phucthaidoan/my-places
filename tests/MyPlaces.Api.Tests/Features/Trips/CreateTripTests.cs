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

public class CreateTripTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task CreateTrip_Authenticated_ReturnsTrip()
    {
        await RegisterAndLoginAsync(email: "trip-create1@test.com", username: "trip-create1");

        var req = new CreateTripRequest(
            Name: "Summer",
            City: "Hanoi",
            Country: "VN",
            StartDate: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate: new DateTime(2026, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            Visibility: TripVisibility.Private);

        var response = await Client.PostAsJsonAsync("/api/v1/trips", req);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson);
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Summer");
        result.Value.City.Should().Be("Hanoi");
        result.Value.Visibility.Should().Be(TripVisibility.Private);
        result.Value.Places.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateTrip_PublicLink_GeneratesShareToken()
    {
        await RegisterAndLoginAsync(email: "trip-create2@test.com", username: "trip-create2");

        var req = new CreateTripRequest(
            "Pub", "HCMC", null, null, null, TripVisibility.PublicLink);

        var response = await Client.PostAsJsonAsync("/api/v1/trips", req);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<Result<TripDetailResponse>>(ApiJson);
        result!.Value!.Visibility.Should().Be(TripVisibility.PublicLink);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var trip = await db.Trips.SingleAsync(t => t.Name == "Pub");
        trip.ShareToken.Should().NotBeNullOrEmpty();
        trip.Visibility.Should().Be(Entities.TripVisibility.PublicLink);
    }

    [Fact]
    public async Task CreateTrip_Unauthenticated_Returns401()
    {
        Client.DefaultRequestHeaders.Clear();
        var req = new CreateTripRequest("T", "C", null, null, null);

        var response = await Client.PostAsJsonAsync("/api/v1/trips", req);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
