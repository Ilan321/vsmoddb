namespace VsModDb.Models.Mods;

public class ModCommentDto
{
    public string Author { get; set; }
    public string Comment { get; set; }
    public ModCommentContentType ContentType { get; set; }
    public DateTime TimeCreatedUtc { get; set; }
    public DateTime? TimeUpdatedUtc { get; set; }
}
