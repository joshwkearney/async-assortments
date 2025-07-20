using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Produces a set intersection between two sequences using the provided key selector to compare elements.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <param name="selector">A selector function used to extract keys from elements.</param>
    /// <param name="comparer">The equality comparer used to compare keys.</param>
    /// <typeparam name="TSource">The type of the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <remarks>
    ///     <para>Uses the provided equality comparer to compare keys.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TKey> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {
        
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        
        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }
        
        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }
        
        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }
        
        var resultOp = new JoinOperator<TSource, TKey, TKey>(
            source.GetScheduleMode().MakeUnordered(),
            source.GetMaxConcurrency(),
            source,
            second,
            selector,
            x => x,
            comparer);
        
        return resultOp
            .Select(pair => pair.first)
            .DistinctBy(selector);
    }

    /// <summary>
    ///     Produces a set intersection between two sequences using the provided key selector to compare elements.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <param name="selector">A selector function used to extract keys from elements.</param>
    /// <typeparam name="TSource">The type of the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <remarks>
    ///     <para>Uses the default equality comparer to compare keys.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TKey> second,
        Func<TSource, TKey> selector) {

        return source.IntersectBy(second, selector, EqualityComparer<TKey>.Default);
    }

    /// <summary>
    ///     Produces a set intersection between two sequences using the provided key selector to compare elements.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <param name="selector">A selector function used to extract keys from elements.</param>
    /// <param name="comparer">The equality comparer used to compare keys.</param>
    /// <typeparam name="TSource">The type of the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <remarks>
    ///     <para>Uses the provided equality comparer to compare keys.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TKey> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    /// <summary>
    ///     Produces a set intersection between two sequences using the provided key selector to compare elements.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <param name="selector">A selector function used to extract keys from elements.</param>
    /// <typeparam name="TSource">The type of the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <remarks>
    ///     <para>Uses the default equality comparer to compare keys.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TKey> second,
        Func<TSource, TKey> selector) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }

    /// <summary>
    ///     Produces a set intersection between two sequences using the provided key selector to compare elements.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <param name="selector">A selector function used to extract keys from elements.</param>
    /// <param name="comparer">The equality comparer used to compare keys.</param>
    /// <typeparam name="TSource">The type of the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <remarks>
    ///     <para>Uses the provided equality comparer to compare keys.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TKey> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {
        
        return source.IntersectBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    /// <summary>
    ///     Produces a set intersection between two sequences using the provided key selector to compare elements.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <param name="selector">A selector function used to extract keys from elements.</param>
    /// <typeparam name="TSource">The type of the source sequence.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <remarks>
    ///     <para>Uses the default equality comparer to compare keys.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TKey> second,
        Func<TSource, TKey> selector) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }
}