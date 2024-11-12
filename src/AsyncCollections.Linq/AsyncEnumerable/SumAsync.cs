using System.Numerics;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {  
    public static ValueTask<T> SumAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) where T : INumber<T> {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        return sequence.AggregateAsync(T.AdditiveIdentity, (x, y) => x + y, cancellationToken);
    }

    public static async ValueTask<float> SumAsync(
        this IAsyncEnumerable<float> sequence,
        CancellationToken cancellationToken = default) {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        return (float)(await sequence.AggregateAsync(0d, (x, y) => x + y, cancellationToken));
    }
}