namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Returns the last element in a sequence.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">The sequence contains no elements.</exception>
    public static ValueTask<TSource> LastAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return LastHelper(source, cancellationToken);        
    }

    private static async ValueTask<TSource> LastHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken) {

        await using var iterator = source.GetAsyncEnumerator(cancellationToken);

        if (!await iterator.MoveNextAsync()) {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        var last = iterator.Current;

        while (await iterator.MoveNextAsync()) {
            last = iterator.Current;
        }

        return last;
    }

    /// <summary>
    ///     Returns the last element in a sequence that matches a given predicate.
    /// </summary>
    /// <param name="predicate">A function that determines which elements can be returned.</param>
    /// <inheritdoc cref="LastAsync{TSource}(IAsyncEnumerable{TSource}, CancellationToken)"/>
    public static ValueTask<TSource> LastAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        return source.Where(predicate).LastAsync(cancellationToken);
    }
}