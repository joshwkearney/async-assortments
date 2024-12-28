using AsyncAssortments.Operators;
using System.Collections;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Sort the elements of a sequence in ascending order according to a key and using the provided comparer
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="keySelector">A function that extracts a key from each element</param>
    /// <param name="comparer">A comparer that defines the sort order</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
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

    /// <summary>
    ///     Sort the elements of a sequence in ascending order according to a key
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="keySelector">A function that extracts a key from each element</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
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

    /// <summary>
    ///     Sort the elements of a sequence in descending order according to a key and using the provided comparer
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="keySelector">A function that extracts a key from each element</param>
    /// <param name="comparer">A comparer that defines the sort order</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
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

    /// <summary>
    ///     Sort the elements of a sequence in descending order according to a key
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="keySelector">A function that extracts a key from each element</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
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