using System.Numerics;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static ValueTask<TSource> MinAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) where TSource : INumber<TSource> {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return source.AggregateAsync(TSource.Min, cancellationToken);
    }

    public static ValueTask<TSource> MaxAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) where TSource : INumber<TSource> {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return source.AggregateAsync(TSource.Max, cancellationToken);
    }
}