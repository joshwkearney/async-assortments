namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Asynchronously applies an accumulator function over a sequence. The specified 
    ///     seed value is used as the initial accumulator value.
    /// </summary>
    /// <param name="seed">The initial accumulator value.</param>
    /// <param name="accumulator">An accumulator function to be invoked on each element.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>A <see cref="ValueTask" /> representing final accumulator value.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<TAccumulate> AggregateAsync<TSource, TAccumulate>(
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

        return Helper();

        async ValueTask<TAccumulate> Helper() {
            await foreach (var item in source.WithCancellation(cancellationToken)) {
                seed = accumulator(seed, item);
            }

            return seed;
        }
    }

    /// <summary>Asynchronously applies an accumulator function over a sequence.</summary>
    /// <param name="reducer">An accumulator function to be invoked on each element.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>A <see cref="ValueTask" /> representing final accumulator value.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<TSource> AggregateAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TSource, TSource> reducer,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (reducer == null) {
            throw new ArgumentNullException(nameof(reducer));
        }

        return Helper();

        async ValueTask<TSource> Helper() {
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
}