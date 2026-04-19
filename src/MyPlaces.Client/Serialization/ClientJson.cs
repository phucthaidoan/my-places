using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyPlaces.Client.Serialization;

public static class ClientJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}
