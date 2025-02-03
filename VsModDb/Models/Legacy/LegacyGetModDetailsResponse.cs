namespace VsModDb.Models.Legacy;

public class LegacyGetModDetailsResponse : BaseLegacyResponse
{
    public LegacyModDetails? Mod { get; init; }
}