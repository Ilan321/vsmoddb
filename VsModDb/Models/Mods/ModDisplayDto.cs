namespace VsModDb.Models.Mods;

public class ModDisplayDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string Author { get; set; }
    public string? UrlAlias { get; set; }
    public string? Summary { get; set; }
    public int Downloads { get; set; }
    public int Comments { get; set; }
}
