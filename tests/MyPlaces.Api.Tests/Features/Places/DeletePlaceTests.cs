using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Tests.Features.Places;

public class DeletePlaceTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task DeletePlace_OwnPlace_ReturnsNoContent()
    {
        await RegisterAndLoginAsync(email: "delete1@test.com", username: "delete1");

        var createResp = await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "To Delete", "1 St", "Hanoi", null, null, null, null));
        var placeId = (await createResp.Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        var response = await Client.DeleteAsync($"/api/v1/places/{placeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await Client.GetAsync($"/api/v1/places/{placeId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePlace_NotFound_Returns404()
    {
        await RegisterAndLoginAsync(email: "delete2@test.com", username: "delete2");

        var response = await Client.DeleteAsync($"/api/v1/places/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePlace_OtherUsersPlace_Returns404()
    {
        await RegisterAndLoginAsync(email: "delete3a@test.com", username: "delete3a");
        var createResp = await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Private", "1 St", "Hanoi", null, null, null, null));
        var placeId = (await createResp.Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        Client.DefaultRequestHeaders.Clear();
        await RegisterAndLoginAsync(email: "delete3b@test.com", username: "delete3b");

        var response = await Client.DeleteAsync($"/api/v1/places/{placeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
