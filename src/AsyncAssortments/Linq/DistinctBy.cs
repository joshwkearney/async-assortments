using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Returns the distinct elements in a sequence, using the provided key selector to
    ///     determine if two elements are equal.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <typeparam name="TSource">The type of the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <returns>A sequence containing only the distinct elements of the original sequence.</returns>
    public static IAsyncEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {

        var totalComparer = new KeyEqualityComparer<TSource, TKey>(keySelector, EqualityComparer<TKey>.Default);

        return source.Distinct(totalComparer);
    }
    
    /// <summary>
    ///     Returns the distinct elements in a sequence, using the provided key selector and key comparer to
    ///     determine if two elements are equal.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="keySelector">A function to extract the key from each element.</param>
    /// <param name="comparer">An equality comparer used to determine if two keys are equal.</param>
    /// <inheritdoc cref="DistinctBy{TSource,TKey}(IAsyncEnumerable{TSource},Func{TSource,TKey})"/>
    public static IAsyncEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey> comparer) {

        var totalComparer = new KeyEqualityComparer<TSource, TKey>(keySelector, comparer);

        return source.Distinct(totalComparer);
    }
}