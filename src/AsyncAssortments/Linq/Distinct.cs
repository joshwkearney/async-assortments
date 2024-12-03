using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Distinct<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEqualityComparer<TSource> comparer) {
        
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        if (source is IDistinctOperator<TSource> op) {
            return op.Distinct(comparer);
        }

        return new DistinctOperator<TSource>(
            source.GetScheduleMode(),
            source,
            comparer);
    }

    public static IAsyncEnumerable<TSource> Distinct<TSource>(
        this IAsyncEnumerable<TSource> source) {

        return source.Distinct(EqualityComparer<TSource>.Default);
    }
}