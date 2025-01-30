using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace VsModDb.Extensions;

public static class EnumerableExtensions
{
    public static async IAsyncEnumerable<TProjected> SelectAsync<T, TProjected>(
        this IEnumerable<T> range,
        Func<T, Task<TProjected>> asyncSelector
    )
    {
        foreach (var item in range)
        {
            var projected = await asyncSelector(item);

            yield return projected;
        }
    }

    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncRange, CancellationToken cancellationToken = default)
    {
        var list = new List<T>();

        await foreach (var item in asyncRange.WithCancellation(cancellationToken))
        {
            list.Add(item);

            cancellationToken.ThrowIfCancellationRequested();
        }

        return list;
    }
}
