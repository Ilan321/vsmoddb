using VsModDb.Data.Entities.Mods;

namespace VsModDb.Models.Mods;

public class ModDetailsDto
{
    /// <inheritdoc cref="Mod.Id"/>
    public int Id { get; set; }

    /// <inheritdoc cref="Mod.Name"/>
    public required string Name { get; set; }

    /// <inheritdoc cref="Mod.Summary"/>
    public required string Summary { get; set; }

    /// <inheritdoc cref="Mod.UrlAlias"/>
    public string? UrlAlias { get; set; }

    /// <inheritdoc cref="Mod.TimeCreatedUtc"/>
    public required DateTime TimeCreatedUtc { get; set; }

    /// <inheritdoc cref="Mod.TimeUpdatedUtc"/>
    public required DateTime TimeUpdatedUtc { get; set; }

    /// <inheritdoc cref="Mod.Description"/>
    public string? Description { get; set; }

    /// <inheritdoc cref="Mod.Tags"/>
    public List<ModTagDto> Tags { get; set; }

    public required string Author { get; set; }
    public required string Side { get; set; }
    public int Downloads { get; set; }
    public int Follows { get; set; }
}