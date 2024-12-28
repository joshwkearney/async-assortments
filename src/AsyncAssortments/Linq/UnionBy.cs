using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Produces a set union between two sequences using the provided key selector to compare elements
    /// </summary>
    /// <remarks>Uses the provided equality comparer to compare elements</remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="selector">A selector function used to extract keys from elements</param>
    /// <param name="comparer">The equality comparer used to compare keys</param>
    public static IAsyncEnumerable<TSource> UnionBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        var totalComparer = new KeyEqualityComparer<TSource, TKey>(selector, comparer);

        return source.Union(second, totalComparer);
    }

    /// <summary>
    ///     Produces a set union between two sequences using the provided key selector to compare elements
    /// </summary>
    /// <remarks>Uses the provided equality comparer to compare elements</remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="selector">A selector function used to extract keys from elements</param>
    public static IAsyncEnumerable<TSource> UnionBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> selector) {

        return source.UnionBy(second, selector, EqualityComparer<TKey>.Default);
    }

    /// <inheritdoc cref="UnionBy{TSource,TKey}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSource},Func{TSource,TKey},IEqualityComparer{TKey})" />
    public static IAsyncEnumerable<TSource> UnionBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.UnionBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    /// <inheritdoc cref="UnionBy{TSource,TKey}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSource},Func{TSource,TKey})" />
    public static IAsyncEnumerable<TSource> UnionBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        Func<TSource, TKey> selector) {

        return source.UnionBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }

    /// <inheritdoc cref="UnionBy{TSource,TKey}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSource},Func{TSource,TKey},IEqualityComparer{TKey})" />
    public static IAsyncEnumerable<TSource> UnionBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.UnionBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    /// <inheritdoc cref="UnionBy{TSource,TKey}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSource},Func{TSource,TKey})" />
    public static IAsyncEnumerable<TSource> UnionBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        Func<TSource, TKey> selector) {

        return source.UnionBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }
}