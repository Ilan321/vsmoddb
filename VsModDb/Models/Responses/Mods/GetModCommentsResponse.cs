using VsModDb.Models.Mods;

namespace VsModDb.Models.Responses.Mods;

public class GetModCommentsResponse
{
    public int TotalComments { get; set; }
    public List<ModCommentDto> Comments { get; set; }
}
