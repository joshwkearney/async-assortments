using System.Numerics;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static ValueTask<T> MinAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) where T : INumber<T> {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        return sequence.AggregateAsync(T.Min, cancellationToken);
    }

    public static ValueTask<T> MaxAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) where T : INumber<T> {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        return sequence.AggregateAsync(T.Max, cancellationToken);
    }
}