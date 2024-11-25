using AsyncCollections.Linq.Operators;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Projects each element into an <see cref="IAsyncEnumerable{T}" /> and 
    ///     flattens the resulting sequences into one sequence.
    /// </summary>
    /// <param name="selector">
    ///     A function that projects each element into an 
    ///     <see cref="IAsyncEnumerable{T}" />.
    /// </param>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> whose elements are the result of
    ///     invoking the one-to-many transformation function on each element.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, IAsyncEnumerable<TResult>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source)); 
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        var pars = source.GetPipelineExecution();

        return new FlattenOperator<TResult>(pars, source.Select(selector));
    }

    /// <summary>
    ///     Projects each element into an <see cref="IEnumerable{T}" /> and 
    ///     flattens the resulting sequences into one sequence.
    /// </summary>
    /// <param name="selector">
    ///     A function that projects each element into an 
    ///     <see cref="IEnumerable{T}" />.
    /// </param>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> whose elements are the result of
    ///     invoking the one-to-many transformation function on each element.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, IEnumerable<TResult>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        var pars = source.GetPipelineExecution();

        return new FlattenEnumerablesOperator<TResult>(pars, source.Select(selector));
    }
}