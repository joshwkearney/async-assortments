using AsyncAssortments.Operators;

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

        var mode = source.GetScheduleMode().MakeOrdered();

        return new SortingOperator<TSource>(
            mode,
            source,
            comparer);
    }

    public static IOrderedAsyncEnumerable<TSource> Order<TSource>(
        this IAsyncEnumerable<TSource> source) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
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

        comparer = Comparer<TSource>.Create((x, y) => comparer.Compare(y, x));
        var mode = source.GetScheduleMode().MakeOrdered();

        return new SortingOperator<TSource>(
            mode,
            source,
            comparer);
    }

    public static IOrderedAsyncEnumerable<TSource> OrderDescending<TSource>(
        this IAsyncEnumerable<TSource> source) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var mode = source.GetScheduleMode().MakeOrdered();

        return new SortingOperator<TSource>(
            mode,
            source,
            Comparer<TSource>.Default);
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
        var mode = source.GetScheduleMode().MakeOrdered();

        return new SortingOperator<TSource>(
            mode,
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
        var mode = source.GetScheduleMode().MakeOrdered();

        return new SortingOperator<TSource>(
            mode,
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
        var mode = source.GetScheduleMode().MakeOrdered();

        return new SortingOperator<TSource>(
            mode,
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
        var mode = source.GetScheduleMode().MakeOrdered();

        return new SortingOperator<TSource>(
            mode,
            source,
            totalComparer);
    }
}