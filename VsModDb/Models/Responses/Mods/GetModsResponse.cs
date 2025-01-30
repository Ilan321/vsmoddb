using VsModDb.Models.Mods;

namespace VsModDb.Models.Responses.Mods;

public class GetModsResponse
{
    public required int TotalMods { get; init; }
    public required List<ModDisplayDto> Mods { get; init; }
}
