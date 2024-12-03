using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

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
        IEqualityComparer<TSource> comparer,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        if (source is IToHashSetOperator<TSource> op) {
            return op.ToHashSetAsync(comparer, cancellationToken);
        }

        return ToHashSetHelper(source, comparer, cancellationToken);
    }

    /// <inheritdoc cref="ToHashSetAsync{TSource}(IAsyncEnumerable{TSource}, IEqualityComparer{TSource}, CancellationToken)" />
    public static ValueTask<HashSet<TSource>> ToHashSetAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IToHashSetOperator<TSource> op) {
            return op.ToHashSetAsync(EqualityComparer<TSource>.Default, cancellationToken);
        }

        return ToHashSetHelper(source, EqualityComparer<TSource>.Default, cancellationToken);
    }

    private static async ValueTask<HashSet<TSource>> ToHashSetHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEqualityComparer<TSource> comparer,
        CancellationToken cancellationToken) {

        var list = new HashSet<TSource>(comparer);

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            list.Add(item);
        }

        return list;
    }
}