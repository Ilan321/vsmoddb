namespace VsModDb.Models.Legacy;

public class LegacyGetGameVersionsResponse : BaseLegacyResponse
{
    public required List<LegacyTag> GameVersions { get; set; }
}
