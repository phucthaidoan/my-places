namespace MyPlaces.Api.Common.Entities;

public class PlacePhoto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlaceId { get; set; }
    public string PhotoUrl { get; set; } = default!;
    public bool IsExternal { get; set; }
    public int SortOrder { get; set; }

    public Place Place { get; set; } = default!;
}
