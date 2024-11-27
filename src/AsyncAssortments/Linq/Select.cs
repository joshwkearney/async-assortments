using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>Projects each element of a sequence into a new form.</summary>
    /// <param name="selector">A transformation function to apply to each source element.</param>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> whose elements are the result of invoking
    ///     the transform function on each element of source.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <seealso cref="AsyncSelect{TSource, TResult}(IAsyncEnumerable{TSource}, Func{TSource, ValueTask{TResult}})"/>
    public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, TResult> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        // Try to compose with a previous operator
        if (source is ISelectOperator<TSource> selectOp) {
            return selectOp.Select(selector);
        }

        var pars = source.GetScheduleMode();

        return new SelectWhereOperator<TSource, TResult>(pars, source, x => new(true, selector(x)));
    }
}