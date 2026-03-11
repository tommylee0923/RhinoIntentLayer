using System.Text.Json;
using System.Text.Json.Serialization;
using Intent.Contract.Models;

namespace Intent.Contract.Serialization
{
    public static class IntentJson
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string SerializeWallIntent(WallIntent intent)
        {
            return JsonSerializer.Serialize(intent, Options);
        }

        public static WallIntent DeserializeWallIntent(string json)
        {
            return JsonSerializer.Deserialize<WallIntent>(json, Options);
        }
    }
}