namespace MyPlaces.Api.Features.Places;
public static class DeletePlace
{
    public static IResult Handle(Guid id) => Results.StatusCode(501);
}
