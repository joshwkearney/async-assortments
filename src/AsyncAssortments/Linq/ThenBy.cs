using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Performs a subsequent ordering of the elements of a sequence in ascending order according to a key, and using the provided comparer
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="keySelector">A function to extract a key from each element</param>
    /// <param name="comparer">A comparer that defines the ordering</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
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

    /// <summary>
    ///     Performs a subsequent ordering of the elements of a sequence in ascending order according to a key
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="keySelector">A function to extract a key from each element</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IOrderedAsyncEnumerable<TSource> ThenBy<TSource, TKey>(
        this IOrderedAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {

        return source.ThenBy(keySelector, Comparer<TKey>.Default);
    }

    /// <summary>
    ///     Performs a subsequent ordering of the elements of a sequence in descending order according to a key, and using the provided comparer
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="keySelector">A function to extract a key from each element</param>
    /// <param name="comparer">A comparer that defines the ordering</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
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

    /// <summary>
    ///     Performs a subsequent ordering of the elements of a sequence in descending order according to a key
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="keySelector">A function to extract a key from each element</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IOrderedAsyncEnumerable<TSource> ThenByDescending<TSource, TKey>(
        this IOrderedAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {

        return source.ThenByDescending(keySelector, Comparer<TKey>.Default);
    }
}