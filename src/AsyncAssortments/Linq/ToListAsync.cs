using System.Threading;
using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Collects a sequence into a <see cref="List{T}"/>.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> that completes when the sequence has been 
    ///     asynchronously enumerated and collected into a list.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<List<TSource>> ToListAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IToListOperator<TSource> op) {
            return op.ToListAsync(cancellationToken);
        }

        return ToListHelper(source, cancellationToken);        
    }

    private static async ValueTask<List<TSource>> ToListHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken) {

        var list = new List<TSource>();

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            list.Add(item);
        }

        return list;
    }
}