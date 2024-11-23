namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Returns the first element in a sequence.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">The sequence contains no elements.</exception>
    public static ValueTask<TSource> FirstAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return FirstHelper(source, cancellationToken);       
      
    }

    private static async ValueTask<TSource> FirstHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken) {

        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
        var worked = await enumerator.MoveNextAsync();

        // We got the first element (or not), so cancel the rest of it
        tokenSource.Cancel();

        if (!worked) {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        return enumerator.Current;
    }

    /// <summary>
    ///     Returns the first element in a sequence that matches a given predicate.
    /// </summary>
    /// <param name="predicate">A function that determines which elements can be returned.</param>
    /// <inheritdoc cref="FirstAsync{TSource}(IAsyncEnumerable{TSource}, CancellationToken)"/>
    public static ValueTask<TSource> FirstAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        return source.Where(predicate).FirstAsync(cancellationToken);
    }
}