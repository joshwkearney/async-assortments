namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>Correlates the elements of two sequences based on matching keys.</summary>
    /// <param name="inner">The sequence to join to the first sequence.</param>
    /// <param name="outerKeySelector">
    ///     A function to extract the join key from each element of the first sequence.
    /// </param>
    /// <param name="innerKeySelector">
    ///     A function to extract the join key from each element of the second sequence.
    /// </param>
    /// <param name="resultSelector">
    ///     An async function to create a result element from two matching elements.
    /// </param>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}"/> that has elements of type TResult that are 
    ///     obtained by performing an inner join on two sequences.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TResult> AsyncJoin<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, ValueTask<TResult>> resultSelector) where TKey : notnull {

        if (outer == null) {
            throw new ArgumentNullException(nameof(outer));
        }

        if (inner == null) {
            throw new ArgumentNullException(nameof(inner));
        }

        if (outerKeySelector == null) {
            throw new ArgumentNullException(nameof(outerKeySelector));
        }

        if (innerKeySelector == null) {
            throw new ArgumentNullException(nameof(innerKeySelector));
        }

        if (resultSelector == null) {
            throw new ArgumentNullException(nameof(resultSelector));
        }

        return outer
            .Join(inner, outerKeySelector, innerKeySelector, resultSelector)
            .AsyncSelect(x => x);
    }
}