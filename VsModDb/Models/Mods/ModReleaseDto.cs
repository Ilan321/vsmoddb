namespace VsModDb.Models.Mods;

public class ModReleaseDto
{
    public string FileName { get; set; }
    public int Downloads { get; set; }
    public List<string> GameVersions { get; set; }
    public string ModId { get; set; }
    public string ModVersion { get; set; }
    public DateTime TimeCreatedUtc { get; set; }
}
