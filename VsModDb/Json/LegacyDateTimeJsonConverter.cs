using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using VsModDb.Extensions;

namespace VsModDb.Json;

public class LegacyDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var val = reader.GetString();

        if (string.IsNullOrWhiteSpace(val))
        {
            return default;
        }

        return LegacyApiExtensions.ParseDateTime(val);
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStringValue(value.ToString(LegacyApiExtensions.DateTimeFormat, CultureInfo.InvariantCulture));
    }
}
