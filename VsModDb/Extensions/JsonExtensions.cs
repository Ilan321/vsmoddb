using System.Text.Json;
using System.Text.Json.Serialization;

namespace VsModDb.Extensions;

public static class JsonExtensions
{
    public static readonly JsonSerializerOptions Default = new JsonSerializerOptions().ConfigureDefaults();

    public static JsonSerializerOptions ConfigureDefaults(this JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));

        return options;
    }
}
