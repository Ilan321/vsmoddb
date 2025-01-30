using Microsoft.AspNetCore.Http;
using System.Globalization;
using VsModDb.Models.Legacy;

namespace VsModDb.Extensions;

public static class LegacyApiExtensions
{
    public const string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

    public static DateTime ParseDateTime(string dt) => DateTime.ParseExact(dt, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

    public static void EnsureSuccessStatusCode(this BaseLegacyResponse? response)
    {
        if (response?.IsSuccess == true) return;

        throw new HttpRequestException($"Non-success status code returned from legacy moddb API: {response?.StatusCode ?? "response is null"}");
    }
}
