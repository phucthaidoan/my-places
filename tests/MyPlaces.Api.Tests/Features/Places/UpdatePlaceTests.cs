using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Tests.Features.Places;

public class UpdatePlaceTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task UpdatePlace_OwnPlace_ReturnsUpdatedPlace()
    {
        await RegisterAndLoginAsync(email: "update1@test.com", username: "update1");

        var createResp = await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Old Name", "Old Address", "Hanoi", null, null, null, null));
        var placeId = (await createResp.Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        var updateReq = new UpdatePlaceRequest(
            Name: "New Name", Address: "New Address", City: "HCMC",
            Country: "Vietnam", Latitude: 10.77, Longitude: 106.69, Note: "Updated");

        var response = await Client.PutAsJsonAsync($"/api/v1/places/{placeId}", updateReq);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<PlaceResponse>>();
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("New Name");
        result.Value.City.Should().Be("HCMC");
        result.Value.Note.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdatePlace_NotFound_Returns404()
    {
        await RegisterAndLoginAsync(email: "update2@test.com", username: "update2");

        var response = await Client.PutAsJsonAsync($"/api/v1/places/{Guid.NewGuid()}",
            new UpdatePlaceRequest("Name", "Addr", "City", null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePlace_OtherUsersPlace_Returns404()
    {
        await RegisterAndLoginAsync(email: "update3a@test.com", username: "update3a");
        var createResp = await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Place", "Addr", "City", null, null, null, null));
        var placeId = (await createResp.Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        Client.DefaultRequestHeaders.Clear();
        await RegisterAndLoginAsync(email: "update3b@test.com", username: "update3b");

        var response = await Client.PutAsJsonAsync($"/api/v1/places/{placeId}",
            new UpdatePlaceRequest("Hacked", "Addr", "City", null, null, null, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
