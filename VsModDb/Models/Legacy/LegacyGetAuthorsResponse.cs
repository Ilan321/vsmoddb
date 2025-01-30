namespace VsModDb.Models.Legacy;

public class LegacyGetAuthorsResponse : BaseLegacyResponse
{
    public List<LegacyAuthor>? Authors { get; set; }
}

public class LegacyAuthor
{
    public int UserId { get; set; }
    public string Name { get; set; }
}
