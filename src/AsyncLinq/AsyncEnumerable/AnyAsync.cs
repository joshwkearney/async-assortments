namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Asynchronously determines whether a sequence contains any elements.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask"/> representing the result.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="OperationCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<bool> AnyAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return Helper();

        async ValueTask<bool> Helper() {
            await using var enumerator = source.GetAsyncEnumerator(cancellationToken);

            return await enumerator.MoveNextAsync();
        }
    }

    /// <summary>
    ///     Asynchronously determines whether any element of a sequence satisfies a condition.
    /// </summary>
    /// <param name="predicate">The condition to search for</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>A <see cref="ValueTask"/> representing the result.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="OperationCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<bool> AnyAsync<TSource>(
       this IAsyncEnumerable<TSource> source,
       Func<TSource, bool> predicate,
       CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        return source.Where(predicate).AnyAsync(cancellationToken);
    }
}