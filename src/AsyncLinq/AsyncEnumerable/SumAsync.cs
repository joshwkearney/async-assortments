using System.Numerics;

namespace AsyncLinq;

public static partial class AsyncEnumerable {  
    public static ValueTask<TSource> SumAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) where TSource : INumber<TSource> {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return source.AggregateAsync(TSource.AdditiveIdentity, (x, y) => x + y, cancellationToken);
    }

    public static async ValueTask<float> SumAsync(
        this IAsyncEnumerable<float> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return (float)(await source.AggregateAsync(0d, (x, y) => x + y, cancellationToken));
    }
}