using System.Numerics;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<E> AggregateAsync<T, E>(
        this IAsyncEnumerable<T> sequence, 
        E seed, 
        Func<E, T, E> reducer,
        CancellationToken cancellationToken = default) {

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            seed = reducer(seed, item);
        }

        return seed;
    }

    public static async ValueTask<T> AggregateAsync<T>(
        this IAsyncEnumerable<T> sequence,
        Func<T, T, T> reducer,
        CancellationToken cancellationToken = default) {

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

    public static async ValueTask<float> SumAsync(
        this IAsyncEnumerable<float> sequence,
        CancellationToken cancellationToken = default) {

        return (float)(await sequence.AggregateAsync(0d, (x, y) => x + y, cancellationToken));
    }

    public static ValueTask<T> MaxAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) where T : INumber<T> {

        return sequence.AggregateAsync(T.Max, cancellationToken);
    }

    public static ValueTask<T> MinAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) where T : INumber<T> {

        return sequence.AggregateAsync(T.Min, cancellationToken);
    }

    public static ValueTask<T> SumAsync<T>(
    this IAsyncEnumerable<T> sequence,
    CancellationToken cancellationToken = default) where T : INumber<T> {

        return sequence.AggregateAsync(T.AdditiveIdentity, (x, y) => x + y, cancellationToken);
    }
}