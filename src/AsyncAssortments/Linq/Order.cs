using AsyncAssortments.Operators;
using System.Collections;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {    
    public static IOrderedAsyncEnumerable<TSource> Order<TSource>(
        this IAsyncEnumerable<TSource> source,
        IComparer<TSource> comparer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        if (source is IOrderOperator<TSource> op) {
            return op.Order(comparer);
        }

        return new SortingOperator<TSource>(
            source.GetScheduleMode().MakeOrdered(),
            source,
            comparer);
    }

    public static IOrderedAsyncEnumerable<TSource> Order<TSource>(
        this IAsyncEnumerable<TSource> source) {

        return source.Order(Comparer<TSource>.Default);
    }

    public static IOrderedAsyncEnumerable<TSource> OrderDescending<TSource>(
        this IAsyncEnumerable<TSource> source,
        IComparer<TSource> comparer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        var totalComparer = Comparer<TSource>.Create((x, y) => comparer.Compare(y, x));

        return source.Order(totalComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> OrderDescending<TSource>(
        this IAsyncEnumerable<TSource> source) {

        var comparer = Comparer<TSource>.Create((x, y) => Comparer<TSource>.Default.Compare(y, x));

        return source.Order(comparer);
    }
}