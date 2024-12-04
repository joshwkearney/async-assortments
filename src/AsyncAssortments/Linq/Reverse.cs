using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Reverse<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IReverseOperator<TSource> op) {
            return op.Reverse();
        }

        return new ReverseOperator<TSource>(
            source.GetScheduleMode(),
            source);
    }
}