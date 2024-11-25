
namespace AsyncLinq;

public static partial class AsyncEnumerableExtensions {
    /// <summary>
    ///     Exposes a <see cref="ValueTask{TResult}" /> as an <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> that will yield the result of awaiting the
    ///     provided task as the only item in the sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this ValueTask<TSource> source) {
        // We need to do this because you can call .GetEnumerator() multiple times on a sequence,
        // which will await our ValueTask multiple times
        return source.AsTask().ToAsyncEnumerable();
    }
}
