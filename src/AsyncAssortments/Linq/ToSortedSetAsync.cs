using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Collects a sequence into a <see cref="SortedSet{T}"/>.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <param name="comparer">
    ///     The comparer to use when sorting.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> that completes when the sequence has been 
    ///     asynchronously enumerated and collected into a sorted set.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<SortedSet<TSource>> ToSortedSetAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        IComparer<TSource> comparer,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IToHashSetOperator<TSource> op) {
            return op.ToHashSetAsync(cancellationToken);
        }

        return ToSortedSetHelper(source, comparer, cancellationToken);
    }

    /// <inheritdoc cref="ToSortedSetAsync{TSource}(IAsyncEnumerable{TSource}, IComparer{TSource}, CancellationToken)" />
    public static ValueTask<SortedSet<TSource>> ToSortedSetAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IToHashSetOperator<TSource> op) {
            return op.ToHashSetAsync(cancellationToken);
        }

        return ToSortedSetHelper(source, Comparer<TSource>.Default, cancellationToken);
    }

    private static async ValueTask<SortedSet<TSource>> ToSortedSetHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        IComparer<TSource> comparer,
        CancellationToken cancellationToken) {

        var list = new SortedSet<TSource>();

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            list.Add(item);
        }

        return list;
    }
}