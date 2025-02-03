using ilandev.Extensions.Configuration;

namespace VsModDb.Models.Options;

public class LegacyClientOptions : IAppOptions
{
    public static string Section => "LegacyClient";

    /// <summary>
    /// Gets whether to redirect API calls to the legacy API instead of using the new database.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Gets the base address of the legacy ModDB.
    /// </summary>
    public required string BaseAddress { get; init; }

    /// <summary>
    /// Enables periodic fetching of mods from the legacy API to keep the cache hydrated.
    /// </summary>
    public bool EnablePeriodicModFetch { get; init; }
}
