using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Appends a value to the end of the sequence.
    /// </summary>
    /// <param name="source">The original sequence.</param>
    /// <param name="element">The value to append to the source sequence.</param>
    /// <remarks>The resulting sequence will preserve the order of its elements.</remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> Append<TSource>(
        this IAsyncEnumerable<TSource> source, 
        TSource element) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IConcatEnumerablesOperator<TSource> concatOp) {
            return concatOp.ConcatEnumerables([], [element]);
        }

        var pars = source.GetScheduleMode().MakeOrdered();
        var maxConcurrency = source.GetMaxConcurrency();

        return new ConcatEnumerablesOperator<TSource>(pars, maxConcurrency, source, [], [element]);
    }
}