
using AsyncCollections.Linq.Operators;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerableExtensions {
    /// <summary>
    ///     Exposes a <see cref="Task{TResult}" /> as an <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> that will yield the result of awaiting the
    ///     provided task as the only item in the sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this Task<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return new WrapAsyncFuncOperator<TSource>(default, _ => new ValueTask<TSource>(source));
    }
}
