using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MagnumOpus.Helpers
{
    /// <summary>
    /// Application constants including server configuration, network ports, and common data structures.
    /// Provides centralized configuration values and reusable arrays for game mechanics.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Server IP address for client connections and service binding.
        /// </summary>
        public static string ServerIP { get; set; } = "192.168.69.1";

        /// <summary>
        /// Port number for Prometheus metrics endpoint.
        /// </summary>
        public static ushort PrometheusPort { get; set; }

        /// <summary>
        /// Port number for login server connections.
        /// </summary>
        public static ushort LoginPort { get; set; }

        /// <summary>
        /// Port number for game server connections.
        /// </summary>
        public static ushort GamePort { get; set; }

        /// <summary>
        /// Direction delta vectors for 8-directional movement in Conquer Online coordinate system.
        /// Indexed by direction enum values for efficient position calculations.
        /// </summary>
        public static readonly Vector2[] DeltaPos =
        [
            new Vector2(0, 1),    // North
            new Vector2(-1, 1),   // NorthWest
            new Vector2(-1, 0),   // West
            new Vector2(-1, -1),  // SouthWest
            new Vector2(0, -1),   // South
            new Vector2(1, -1),   // SouthEast
            new Vector2(1, 0),    // East
            new Vector2(1, 1)     // NorthEast
        ];

        /// <summary>
        /// Standard JSON serialization options for consistent data serialization across the application.
        /// Includes field serialization, case-insensitive properties, and camelCase enum conversion.
        /// </summary>
        public static readonly JsonSerializerOptions serializerOptions = new()
        {
            IncludeFields = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        };
    }
}