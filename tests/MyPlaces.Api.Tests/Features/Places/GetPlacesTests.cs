using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MyPlaces.Shared.Common;
using MyPlaces.Shared.Places;

namespace MyPlaces.Api.Tests.Features.Places;

public class GetPlacesTests(TestWebAppFactory factory) : AuthenticatedTestBase(factory)
{
    [Fact]
    public async Task GetPlaces_ReturnsOnlyOwnPlaces()
    {
        await RegisterAndLoginAsync(email: "getplaces1@test.com", username: "getplaces1");

        await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "My Place", "1 St", "Hanoi", null, null, null, null));

        var response = await Client.GetAsync("/api/v1/places");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Result<List<PlaceSummaryResponse>>>();
        result!.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("My Place");
    }

    [Fact]
    public async Task GetPlaces_FilterByCity_ReturnsMatchingPlaces()
    {
        await RegisterAndLoginAsync(email: "getplaces2@test.com", username: "getplaces2");

        await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Da Nang Place", "1 St", "Da Nang", null, null, null, null));
        await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Hanoi Place", "2 St", "Hanoi", null, null, null, null));

        var response = await Client.GetAsync("/api/v1/places?city=Da+Nang");

        var result = await response.Content.ReadFromJsonAsync<Result<List<PlaceSummaryResponse>>>();
        result!.Value!.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Da Nang Place");
    }

    [Fact]
    public async Task GetPlaces_SearchByName_ReturnsMatchingPlaces()
    {
        await RegisterAndLoginAsync(email: "getplaces3@test.com", username: "getplaces3");

        await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Pho Thin", "1 St", "Hanoi", null, null, null, null));
        await Client.PostAsJsonAsync("/api/v1/places", new CreatePlaceRequest(
            "Bun Bo", "2 St", "Hue", null, null, null, null));

        var response = await Client.GetAsync("/api/v1/places?q=pho");

        var result = await response.Content.ReadFromJsonAsync<Result<List<PlaceSummaryResponse>>>();
        result!.Value!.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Pho Thin");
    }

    [Fact]
    public async Task GetPlaces_Unauthenticated_ReturnsUnauthorized()
    {
        var response = await Client.GetAsync("/api/v1/places");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
