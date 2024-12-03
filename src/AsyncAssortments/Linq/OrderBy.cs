using AsyncAssortments.Operators;
using System.Collections;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {    
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

        return source.Order(totalComparer);
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

        return source.Order(totalComparer);
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

        return source.Order(totalComparer);
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

        return source.Order(totalComparer);
    }
}