using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;


public static partial class AsyncEnumerable {
    /// <summary>
    ///     Filters a sequence using a predicate to select elements.
    /// </summary>
    /// <param name="source">The original sequence.</param>
    /// <param name="predicate">
    ///     A function that determines if an element should remain in
    ///     the sequence.
    /// </param>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> containing all of the 
    ///     elements that were allowed by the predicate.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> Where<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, bool> predicate) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        // Try to compose with a previous operator
        if (source is IWhereOperator<TSource> whereOp) {
            return whereOp.Where(predicate);
        }

        var pars = source.GetScheduleMode();
        var maxConcurrency = source.GetMaxConcurrency();

        return new SelectWhereOperator<TSource, TSource>(pars, maxConcurrency, source, x => new(predicate(x), x));
    }
}