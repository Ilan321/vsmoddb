using System.Text.Json.Serialization;
using VsModDb.Json;

namespace VsModDb.Models.Legacy;

public class LegacyModRelease
{
    public int ReleaseId { get; set; }
    public string MainFile { get; set; }
    public string FileName { get; set; }
    public int FileId { get; set; }
    public int Downloads { get; set; }
    public List<string> Tags { get; set; }
    public string ModIdStr { get; set; }
    public string ModVersion { get; set; }

    [JsonConverter(typeof(LegacyDateTimeJsonConverter))]
    public DateTime Created { get; set; }
}
