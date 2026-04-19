using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Tests.Features.Places;

public class GetPlaceTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task GetPlace_OwnPlace_ReturnsPlace()
    {
        await RegisterAndLoginAsync(email: "getplace1@test.com", username: "getplace1");

        var createResp = await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "My Spot", "1 St", "HCMC", "Vietnam", 10.77, 106.69, "Nice"));
        var created = await createResp.Content.ReadFromJsonAsync<Result<PlaceResponse>>();
        var placeId = created!.Value!.Id;

        var response = await Client.GetAsync($"/api/v1/places/{placeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<PlaceResponse>>();
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(placeId);
        result.Value.Note.Should().Be("Nice");
    }

    [Fact]
    public async Task GetPlace_NotFound_Returns404()
    {
        await RegisterAndLoginAsync(email: "getplace2@test.com", username: "getplace2");

        var response = await Client.GetAsync($"/api/v1/places/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPlace_OtherUsersPlace_Returns404()
    {
        await RegisterAndLoginAsync(email: "getplace3a@test.com", username: "getplace3a");
        var createResp = await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Private Spot", "1 St", "Hanoi", null, null, null, null));
        var placeId = (await createResp.Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        // Switch to a different user
        Client.DefaultRequestHeaders.Clear();
        await RegisterAndLoginAsync(email: "getplace3b@test.com", username: "getplace3b");

        var response = await Client.GetAsync($"/api/v1/places/{placeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
