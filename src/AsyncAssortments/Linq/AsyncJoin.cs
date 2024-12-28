using System.Threading.Channels;
using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Correlates and asynchronously combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <remarks>The provided equality comparer is used to compare keys.</remarks>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <param name="resultSelector">The selector used to combine elements from the two sequences</param>
    /// <param name="comparer">The comparer used to compare keys</param>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<TResult> AsyncJoin<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, ValueTask<TResult>> resultSelector,
        IEqualityComparer<TKey> comparer) {

        return outer
            .Join(inner, outerKeySelector, innerKeySelector, (x, y) => (outer: x, inner: y), comparer)
            .AsyncSelect(pair => resultSelector(pair.outer, pair.inner));
    }

    /// <remarks>The default equality comparer is used to compare keys.</remarks>
    /// <inheritdoc cref="AsyncJoin{TOuter,TInner,TKey,TResult}(IAsyncEnumerable{TOuter},IAsyncEnumerable{TInner},Func{TOuter,TKey},Func{TInner,TKey},Func{TOuter,TInner,ValueTask{TResult}},IEqualityComparer{TKey})"/>
    public static IAsyncEnumerable<TResult> AsyncJoin<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, ValueTask<TResult>> resultSelector) {

        return outer
            .Join(inner, outerKeySelector, innerKeySelector, (x, y) => (outer: x, inner: y))
            .AsyncSelect(pair => resultSelector(pair.outer, pair.inner));
    }
}