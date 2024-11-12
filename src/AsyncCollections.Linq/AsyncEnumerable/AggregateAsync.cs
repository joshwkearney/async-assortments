using System.Numerics;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<E> AggregateAsync<T, E>(
        this IAsyncEnumerable<T> sequence, 
        E seed, 
        Func<E, T, E> reducer,
        CancellationToken cancellationToken = default) {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        if (reducer == null) {
            throw new ArgumentNullException(nameof(reducer));
        }

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            seed = reducer(seed, item);
        }

        return seed;
    }

    public static async ValueTask<T> AggregateAsync<T>(
        this IAsyncEnumerable<T> sequence,
        Func<T, T, T> reducer,
        CancellationToken cancellationToken = default) {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        if (reducer == null) {
            throw new ArgumentNullException(nameof(reducer));
        }

        await using var iterator = sequence.GetAsyncEnumerator(cancellationToken);

        var hasFirst = await iterator.MoveNextAsync();
        var first = iterator.Current;

        if (!hasFirst) {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        while (await iterator.MoveNextAsync()) {
            first = reducer(first, iterator.Current);
        }

        return first;
    }
}