using System.Text.Json;
using System.Text.Json.Serialization;

namespace Caching.Serialization;

public static class JsonSerializationOptions
{
    public static JsonSerializerOptions Default { get; }

    static JsonSerializationOptions()
    {
        Default = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };
    }
}
