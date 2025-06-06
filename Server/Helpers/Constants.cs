using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagnumOpus.Helpers
{
    public static class Constants
    {
        public static string ServerIP { get; set; } = "192.168.0.209";
        public static ushort PrometheusPort { get; set; }
        public static ushort LoginPort { get; set; }
        public static ushort GamePort { get; set; }

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