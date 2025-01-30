namespace VsModDb.Models.Mods;

/// <summary>
/// Comments model for the "latest comments" API. Includes the mod details as well.
/// </summary>
public class LatestModCommentDto
{
    public ModCommentDto Comment { get; set; }
    public ModDisplayDto Mod { get; set; }
}
