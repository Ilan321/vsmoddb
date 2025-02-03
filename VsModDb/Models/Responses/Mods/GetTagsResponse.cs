using VsModDb.Models.Mods;

namespace VsModDb.Models.Responses.Mods;

public class GetTagsResponse
{
    public required List<ModTagDto> Tags { get; set; }
    public required List<ModTagDto> GameVersions { get; set; }
}
