using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagnumOpus.Helpers
{
    public static class Constants
    {
        public static string ServerIP;
        public static int PrometheusPort;
        public static int LoginPort;
        public static int GamePort;

        public static readonly Vector2[] DeltaPos = new Vector2[]
        {
            new Vector2(0, 1),
            new Vector2(-1, 1),
            new Vector2(-1, 0),
            new Vector2(-1, -1),
            new Vector2(0, -1),
            new Vector2(1, -1),
            new Vector2(1, 0),
            new Vector2(1, 1)
        };

        public static readonly JsonSerializerOptions serializerOptions = new()
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
    }
}