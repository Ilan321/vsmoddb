namespace VsModDb.Extensions;

public static class StringExtensions
{
    public static string JoinString(this IEnumerable<string> parts, string? separator = default) => string.Join(separator, parts);
}
