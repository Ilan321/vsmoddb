using System.Text.Json;
using System.Text.Json.Serialization;

namespace VsModDb.Json;

public class LegacyZeroableStringConverter : JsonConverter<string>
{
    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var num = reader.GetInt32();

            if (num == 0)
            {
                return null;
            }

            return num.ToString();
        }

        return reader.GetString();
    }

    public override void Write(
        Utf8JsonWriter writer,
        string value,
        JsonSerializerOptions options
    )
    {
        writer.WriteStringValue(value);
    }
}
