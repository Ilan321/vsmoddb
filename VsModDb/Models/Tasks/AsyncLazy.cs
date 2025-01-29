using System.Runtime.CompilerServices;

namespace VsModDb.Models.Tasks;

public class AsyncLazy<T>
{
    private readonly Func<Task<T>> _factory;

    private bool _isValueCreated;
    private T? _value;

    public AsyncLazy(Func<Task<T>> factory)
    {
        _factory = factory;
    }

    public Task<T> Value => GetOrCreateValueAsync();

    private async Task<T> GetOrCreateValueAsync()
    {
        if (_isValueCreated)
        {
            return _value!;
        }

        _value = await _factory();
        _isValueCreated = true;

        return _value;
    }

    public TaskAwaiter<T> GetAwaiter() => Value.GetAwaiter();
}
