using System.Text.Json.Serialization;
using VsModDb.Json;

namespace VsModDb.Models.Legacy;

public class LegacyGetModDetailsResponse : BaseLegacyResponse
{
    public LegacyModDetails? Mod { get; init; }
}

public class LegacyModDetails
{
    public int ModId { get; init; }
    public int AssetId { get; init; }
    public required string Name { get; init; }
    public string? Text { get; init; }
    public required string Author { get; init; }
    public string? UrlAlias { get; init; }
    public string? LogoFileName { get; init; }
    public int Downloads { get; init; }
    public int Follows { get; init; }
    public int Comments { get; init; }

    [JsonConverter(typeof(LegacyDateTimeJsonConverter))]
    public DateTime Created { get; init; }

    [JsonConverter(typeof(LegacyDateTimeJsonConverter))]
    public DateTime LastModified { get; init; }

    public required string[] Tags { get; init; }
}