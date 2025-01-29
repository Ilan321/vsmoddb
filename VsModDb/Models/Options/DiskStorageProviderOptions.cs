using ilandev.Extensions.Configuration;

namespace VsModDb.Models.Options;

public class DiskStorageProviderOptions : IAppOptions
{
    public static string Section => "Files:DiskStorage";

    public required string BasePath { get; init; }
}
