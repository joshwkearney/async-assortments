namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Collects a sequence into an array.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> that completes when the sequence has been 
    ///     asynchronously enumerated and collected into an array.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<TSource[]> ToArrayAsync<TSource>(
        this IAsyncEnumerable<TSource> source, 
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return ToArrayHelper(source, cancellationToken);
    }

    private static async ValueTask<TSource[]> ToArrayHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken) {

        var list = await source.ToListAsync(cancellationToken);

        return list.ToArray();
    }
}