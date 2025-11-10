using Chinook.Domain.Helpers;

namespace Chinook.Domain.Hypermedia;

public interface IEnricher
{
    bool Match(object? value);
    Task ProcessAsync(object? value, CancellationToken ct = default);
}

public abstract class Enricher<T> : IEnricher where T : class, IRepresentation
{
    public virtual bool Match(object? value) => value is T;

    public async Task ProcessAsync(object? value, CancellationToken ct = default)
    {
        if (value is T typed)
        {
            await ProcessAsync(typed, ct);
        }
    }

    protected abstract Task ProcessAsync(T representation, CancellationToken ct);
}

public abstract class ListEnricher<TList> : IEnricher where TList : class
{
    public virtual bool Match(object? value) => value is TList;

    public async Task ProcessAsync(object? value, CancellationToken ct = default)
    {
        if (value is TList typed)
        {
            await ProcessAsync(typed, ct);
        }
    }

    protected abstract Task ProcessAsync(TList representation, CancellationToken ct);
}
