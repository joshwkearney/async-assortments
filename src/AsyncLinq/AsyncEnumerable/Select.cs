using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>Projects each element of a sequence into a new form.</summary>
    /// <param name="selector">A transform function to apply to each source element.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}" /> whose elements are the result of invoking the transform function on each element of source.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, TResult> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        // This is our SelectWhere selector
        SelectWhereResult<TResult> selectWhereFunc(TSource item) => new(true, selector(item));

        // Try to compose with a previous operator
        if (source is ISelectWhereOperator<TSource> selectWhereOp) {
            return selectWhereOp.SelectWhere(selectWhereFunc);
        }

        var pars = source.GetPipelineExecution();

        return new SelectWhereOperator<TSource, TResult>(pars, source, selectWhereFunc);
    }
}