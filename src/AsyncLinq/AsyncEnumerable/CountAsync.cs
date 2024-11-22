using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Counts the number of items in a sequence
    /// </summary>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>The number of items in the sequence</returns>
    /// <exception cref="ArgumentNullException">One of the provided arguments was null</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<int> CountAsync<TSource>(
        this IAsyncEnumerable<TSource> source, 
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is ICountOperator<TSource> op) {
            var count = op.Count();

            if (count >= 0) {
                return new ValueTask<int>(count);
            }
        }

        return Helper();

        async ValueTask<int> Helper() {
            int count = 0;

            await foreach (var item in source.WithCancellation(cancellationToken)) {
                count++;
            }

            return count;
        }
    }

    /// <summary>
    ///     Counts the number of items in a sequence that match a given predicate
    /// </summary>
    /// <param name="predicate">A predicate that defines which items should be counted</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>The number of items in the sequence that match the predicate</returns>
    /// <exception cref="ArgumentNullException">One of the provided arguments was null</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<int> CountAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return source.Where(predicate).CountAsync(cancellationToken);
    }
}