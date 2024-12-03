using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    public static IOrderedAsyncEnumerable<TSource> Then<TSource>(
        this IOrderedAsyncEnumerable<TSource> source,
        IComparer<TSource> comparer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        var totalComparer = Comparer<TSource>.Create((x, y) => {
            // Do this branchless so sorting will be faster
            var comp1 = source.Comparer.Compare(x, y);
            var comp2 = comparer.Compare(x, y);

            return comp1 == 0 ? comp2 : comp1;
        });

        return source.Source.Order(totalComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> ThenDescending<TSource>(
        this IOrderedAsyncEnumerable<TSource> source,
        IComparer<TSource> comparer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        var thenComparer = Comparer<TSource>.Create((x, y) => comparer.Compare(y, x));

        return source.Then(thenComparer);
    }

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

        var thenComparer = Comparer<TSource>.Create((x, y) => {
            return comparer.Compare(keySelector(x), keySelector(y));
        });

        return source.Then(thenComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> ThenBy<TSource, TKey>(
        this IOrderedAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {

        return source.ThenBy(keySelector, Comparer<TKey>.Default);
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

        var thenComparer = Comparer<TSource>.Create((x, y) => {
            return comparer.Compare(keySelector(y), keySelector(x));
        });

        return source.Then(thenComparer);
    }

    public static IOrderedAsyncEnumerable<TSource> ThenByDescending<TSource, TKey>(
        this IOrderedAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {

        return source.ThenByDescending(keySelector, Comparer<TKey>.Default);
    }
}