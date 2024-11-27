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

        if (source is IOrderByOperator<TSource> op) {
            return op.OrderBy(comparer);
        }

        return new SortingOperator<TSource>(
            source.GetScheduleMode().MakeOrdered(),
            source,
            comparer);
    }

    public static IOrderedAsyncEnumerable<TSource> Order<TSource>(
        this IAsyncEnumerable<TSource> source) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IOrderByOperator<TSource> op) {
            return op.OrderBy(Comparer<TSource>.Default);
        }

        var mode = source.GetScheduleMode().MakeOrdered();

        return new SortingOperator<TSource>(
            mode,
            source,
            Comparer<TSource>.Default);
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

        if (source is IOrderByOperator<TSource> op) {
            return op.OrderBy(totalComparer);
        }

        return new SortingOperator<TSource>(
            source.GetScheduleMode().MakeOrdered(),
            source,
            totalComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> OrderDescending<TSource>(
        this IAsyncEnumerable<TSource> source) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var comparer = Comparer<TSource>.Create((x, y) => Comparer<TSource>.Default.Compare(y, x));

        if (source is IOrderByOperator<TSource> op) {
            return op.OrderBy(comparer);
        }

        return new SortingOperator<TSource>(
            source.GetScheduleMode().MakeOrdered(),
            source,
            comparer);
    }

    public static IOrderedAsyncEnumerable<TSource> OrderBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey> comparer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (keySelector == null) {
            throw new ArgumentNullException(nameof(keySelector));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        var totalComparer = Comparer<TSource>.Create((x, y) => comparer.Compare(keySelector(x), keySelector(y)));

        if (source is IOrderByOperator<TSource> op) {
            return op.OrderBy(totalComparer);
        }

        return new SortingOperator<TSource>(
            source.GetScheduleMode().MakeOrdered(),
            source,
            totalComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> OrderBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (keySelector == null) {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var comparer = Comparer<TKey>.Default;
        var totalComparer = Comparer<TSource>.Create((x, y) => comparer.Compare(keySelector(x), keySelector(y)));

        if (source is IOrderByOperator<TSource> op) {
            return op.OrderBy(totalComparer);
        }

        return new SortingOperator<TSource>(
            source.GetScheduleMode().MakeOrdered(),
            source,
            totalComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> OrderByDescending<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IComparer<TKey> comparer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (keySelector == null) {
            throw new ArgumentNullException(nameof(keySelector));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        var totalComparer = Comparer<TSource>.Create((x, y) => comparer.Compare(keySelector(y), keySelector(x)));

        if (source is IOrderByOperator<TSource> op) {
            return op.OrderBy(totalComparer);
        }

        return new SortingOperator<TSource>(
            source.GetScheduleMode().MakeOrdered(),
            source,
            totalComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> OrderByDescending<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (keySelector == null) {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var comparer = Comparer<TKey>.Default;
        var totalComparer = Comparer<TSource>.Create((x, y) => comparer.Compare(keySelector(y), keySelector(x)));

        if (source is IOrderByOperator<TSource> op) {
            return op.OrderBy(totalComparer);
        }

        return new SortingOperator<TSource>(
            source.GetScheduleMode().MakeOrdered(),
            source,
            totalComparer);
    }
}