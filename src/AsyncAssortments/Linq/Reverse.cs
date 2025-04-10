using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Reverses a sequence.
    /// </summary>
    /// <param name="source">The original sequence.</param>
    /// <remarks>The resulting sequence will preserve the order of its elements.</remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> Reverse<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IReverseOperator<TSource> op) {
            return op.Reverse();
        }

        return new ReverseOperator<TSource>(
            source.GetScheduleMode().MakeOrdered(),
            source);
    }
}