namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static async ValueTask<TAccumulate> AggregateAsync<TSource, TAccumulate>(
        this IAsyncEnumerable<TSource> source, 
        TAccumulate seed, 
        Func<TAccumulate, TSource, TAccumulate> accumulator,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (accumulator == null) {
            throw new ArgumentNullException(nameof(accumulator));
        }

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            seed = accumulator(seed, item);
        }

        return seed;
    }

    public static async ValueTask<TSource> AggregateAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TSource, TSource> reducer,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (reducer == null) {
            throw new ArgumentNullException(nameof(reducer));
        }

        await using var iterator = source.GetAsyncEnumerator(cancellationToken);

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