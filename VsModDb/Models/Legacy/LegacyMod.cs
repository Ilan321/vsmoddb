using System.Text.Json.Serialization;
using VsModDb.Json;

namespace VsModDb.Models.Legacy;

public class LegacyMod
{
    public int ModId { get; init; }
    public int AssetId { get; init; }
    public int Downloads { get; init; }
    public int Comments { get; init; }
    public int Follows { get; init; }
    public required string Name { get; init; }
    public required string Summary { get; init; }

    [JsonPropertyName("modidstrs")]
    public required string[] ModIds { get; init; }

    public required string Author { get; init; }
    public string? UrlAlias { get; init; }
    public required string[] Tags { get; init; }
    
    [JsonConverter(typeof(LegacyDateTimeJsonConverter))]
    public required DateTime LastReleased { get; init; }
}