using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {    
    public static IOrderedAsyncEnumerable<TSource> ThenBy<TSource, TKey>(
        this IOrderedAsyncEnumerable<TSource> source,
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

        var totalComparer = Comparer<TSource>.Create((x, y) => {
            // Do this branchless so sorting will be faster
            var comp1 = source.Comparer.Compare(x, y);
            var comp2 = comparer.Compare(keySelector(x), keySelector(y));

            return comp1 == 0 ? comp2 : comp1;
        });

        var mode = source.GetScheduleMode().MakeOrdered();

        return new SortingOperator<TSource>(
            mode,
            source.Source,
            totalComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> ThenBy<TSource, TKey>(
        this IOrderedAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (keySelector == null) {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var mode = source.GetScheduleMode().MakeOrdered();
        var defaultKeyComparer = Comparer<TKey>.Default;

        var totalComparer = Comparer<TSource>.Create((x, y) => {
            // Do this branchless so sorting will be faster
            var comp1 = source.Comparer.Compare(x, y);
            var comp2 = defaultKeyComparer.Compare(keySelector(x), keySelector(y));

            return comp1 == 0 ? comp2 : comp1;
        });

        return new SortingOperator<TSource>(
            mode,
            source.Source,
            totalComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> ThenByDescending<TSource, TKey>(
        this IOrderedAsyncEnumerable<TSource> source,
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

        var totalComparer = Comparer<TSource>.Create((x, y) => {
            // Do this branchless so sorting will be faster
            var comp1 = source.Comparer.Compare(x, y);
            var comp2 = comparer.Compare(keySelector(y), keySelector(x));

            return comp1 == 0 ? comp2 : comp1;
        });

        var mode = source.GetScheduleMode().MakeOrdered();

        return new SortingOperator<TSource>(
            mode,
            source.Source,
            totalComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> ThenByDescending<TSource, TKey>(
        this IOrderedAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (keySelector == null) {
            throw new ArgumentNullException(nameof(keySelector));
        }

        var mode = source.GetScheduleMode().MakeOrdered();
        var defaultKeyComparer = Comparer<TKey>.Default;

        var totalComparer = Comparer<TSource>.Create((x, y) => {
            // Do this branchless so sorting will be faster
            var comp1 = source.Comparer.Compare(x, y);
            var comp2 = defaultKeyComparer.Compare(keySelector(y), keySelector(x));

            return comp1 == 0 ? comp2 : comp1;
        });

        return new SortingOperator<TSource>(
            mode,
            source.Source,
            totalComparer);
    }
}