using VsModDb.Models.Mods;

namespace VsModDb.Models.Requests.Mods;

public class SearchModsRequest
{
    public string? Text { get; init; }
    public ModSortType? Sort { get; init; }
    public ModSortDirection? Direction { get; init; }
    public string? Author { get; init; }
    public string? Side { get; init; }
    public string? GameVersion { get; init; }
    public List<string>? Tags { get; init; }
    public int? Skip { get; init; }
    public int? Take { get; init; }
}