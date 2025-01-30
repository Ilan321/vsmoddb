using System.Text.Json.Serialization;
using VsModDb.Json;

namespace VsModDb.Models.Legacy;

public class LegacyGetCommentsResponse : BaseLegacyResponse
{
    public List<LegacyComment>? Comments { get; set; }
}

public class LegacyComment
{
    public int CommentId { get; set; }
    public int AssetId { get; set; }
    public int UserId { get; set; }
    public required string Text { get; set; }
    
    [JsonConverter(typeof(LegacyDateTimeJsonConverter))]
    public DateTime Created { get; set; }

    [JsonConverter(typeof(LegacyDateTimeJsonConverter))]
    public DateTime LastModified { get; set; }
}
