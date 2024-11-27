namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Asynchronously determines whether all elements of a sequence satisfies a condition.
    /// </summary>
    /// <param name="predicate">The condition to match</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>A <see cref="ValueTask"/> representing the result.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<bool> AllAsync<TSource>(
       this IAsyncEnumerable<TSource> source,
       Func<TSource, bool> predicate,
       CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        return AllHelper(source, predicate, cancellationToken);
    }

    private static async ValueTask<bool> AllHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken) {

        await foreach (var item in source) {
            if (!predicate(item)) {
                return false;
            }
        }

        return true;
    }
}