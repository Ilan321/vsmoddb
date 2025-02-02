using Hangfire.Common;
using VsModDb.Services.LegacyApi;

namespace VsModDb.Services.Jobs;

public class HydrateModDetailsJob(ILegacyApiClient client)
{
    public const string JobId = nameof(HydrateModDetailsJob);

    public Task ExecuteAsync(CancellationToken cancellationToken = default) =>
        client.HydrateModDetailsAsync(cancellationToken);
}
