namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Collects a sequence into a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> that completes when the sequence has been 
    ///     asynchronously enumerated and collected into a hash set.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<HashSet<TSource>> ToHashSetAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return ToHashSetHelper(source, cancellationToken);
    }

    private static async ValueTask<HashSet<TSource>> ToHashSetHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken) {

        var list = new HashSet<TSource>();

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            list.Add(item);
        }

        return list;
    }
}