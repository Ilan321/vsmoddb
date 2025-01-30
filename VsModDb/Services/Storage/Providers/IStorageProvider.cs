namespace VsModDb.Services.Storage.Providers;

public interface IStorageProvider
{
    Task<Stream> GetFileAsync(string fileName, CancellationToken cancellationToken = default);
    Task SaveFileAsync(string assetPath, Stream stream, CancellationToken cancellationToken = default);
    Task<bool> DoesFileExistAsync(string assetPath, CancellationToken cancellationToken = default);
}
