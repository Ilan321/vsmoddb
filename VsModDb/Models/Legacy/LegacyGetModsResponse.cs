namespace VsModDb.Models.Legacy;

public class LegacyGetModsResponse : BaseLegacyResponse
{
    public List<LegacyMod> Mods { get; set; }
}