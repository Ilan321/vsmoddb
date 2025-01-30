using Microsoft.Extensions.Options;
using VsModDb.Models.Options;

namespace VsModDb.Services.Storage.Providers;

public class DiskStorageProvider(IOptions<DiskStorageProviderOptions> options) : IStorageProvider
{
    public Task<Stream> GetFileAsync(string fileName, CancellationToken cancellationToken)
    {
        EnsureBaseDirectoryCreated();

        var path = Path.Combine(options.Value.BasePath, fileName);

        var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

        return Task.FromResult<Stream>(fs);
    }

    public async Task SaveFileAsync(string assetPath, Stream stream, CancellationToken cancellationToken = default)
    {
        EnsureBaseDirectoryCreated();

        var path = Path.Combine(options.Value.BasePath, assetPath);

        var directory = Path.GetDirectoryName(path);

        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new InvalidOperationException($"Invalid asset path: {assetPath}");
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);

        await stream.CopyToAsync(fs, cancellationToken);
    }

    public Task<bool> DoesFileExistAsync(string assetPath, CancellationToken cancellationToken = default) =>
        Task.FromResult(
            File.Exists(Path.Combine(options.Value.BasePath, assetPath))
        );

    private void EnsureBaseDirectoryCreated()
    {
        if (Directory.Exists(options.Value.BasePath))
        {
            return;
        }

        Directory.CreateDirectory(options.Value.BasePath);
    }
}