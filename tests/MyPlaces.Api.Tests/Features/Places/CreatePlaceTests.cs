using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Tests.Features.Places;

public class CreatePlaceTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task CreatePlace_Authenticated_ReturnsPlace()
    {
        await RegisterAndLoginAsync(email: "create1@test.com", username: "createuser1");

        var request = new CreatePlaceRequest(
            Name: "Pho Bo Hue",
            Address: "123 Le Loi",
            City: "Da Nang",
            Country: "Vietnam",
            Latitude: 16.047,
            Longitude: 108.206,
            Note: "Great pho"
        );

        var response = await Client.PostAsJsonAsync("/api/v1/places", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<PlaceResponse>>();
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Pho Bo Hue");
        result.Value.City.Should().Be("Da Nang");
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Photos.Should().BeEmpty();
    }

    [Fact]
    public async Task CreatePlace_Unauthenticated_ReturnsUnauthorized()
    {
        var request = new CreatePlaceRequest(
            Name: "Test Place", Address: "123 St", City: "HCMC",
            Country: null, Latitude: null, Longitude: null, Note: null);

        var response = await Client.PostAsJsonAsync("/api/v1/places", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
