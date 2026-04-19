using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Tests.Features.Places;

public class CopyPlaceTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task CopyPlace_ExistingPlace_CreatesNewPlaceWithSourceId()
    {
        await RegisterAndLoginAsync(email: "copy1a@test.com", username: "copy1a");
        var createResp = await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Original Place", "1 St", "Hanoi", "Vietnam", 21.02, 105.83, "Good spot"));
        var sourceId = (await createResp.Content.ReadFromJsonAsync<Result<PlaceResponse>>())!.Value!.Id;

        Client.DefaultRequestHeaders.Clear();
        await RegisterAndLoginAsync(email: "copy1b@test.com", username: "copy1b");

        var response = await Client.PostAsJsonAsync($"/api/v1/places/{sourceId}/copy", new { });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<PlaceResponse>>();
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Original Place");
        result.Value.SourcePlaceId.Should().Be(sourceId);
        result.Value.Id.Should().NotBe(sourceId);
        result.Value.Photos.Should().BeEmpty();
    }

    [Fact]
    public async Task CopyPlace_NotFound_Returns404()
    {
        await RegisterAndLoginAsync(email: "copy2@test.com", username: "copy2");

        var response = await Client.PostAsJsonAsync($"/api/v1/places/{Guid.NewGuid()}/copy", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
