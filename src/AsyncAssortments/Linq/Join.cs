using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <param name="comparer">The equality comparer used to compare keys</param>
    /// <remarks>The provided equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<(TOuter outer, TInner inner)> Join<TOuter, TInner, TKey>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        IEqualityComparer<TKey> comparer) {

        if (outer == null) {
            throw new ArgumentNullException(nameof(outer));
        }

        if (inner == null) {
            throw new ArgumentNullException(nameof(inner));
        }

        if (outerKeySelector == null) {
            throw new ArgumentNullException(nameof(outerKeySelector));
        }

        if (innerKeySelector == null) {
            throw new ArgumentNullException(nameof(innerKeySelector));
        }
        
        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        if (outer is IAsyncOperator<TOuter> op) {
            outer = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        // For some unfathomable reason, Join() excludes null elements. We only need to do this on one of
        // the sequences to prevent joining on nulls
        outer = outer.Where(x => outerKeySelector(x) != null);
        
        var resultOp = new JoinOperator<TOuter, TInner, TKey>(
            outer.GetScheduleMode(),
            outer,
            inner,
            outerKeySelector,
            innerKeySelector,
            comparer);

        return resultOp;
    }
    
    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <remarks>The default equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<(TOuter outer, TInner inner)> Join<TOuter, TInner, TKey>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector) {

        return outer.Join(inner, outerKeySelector, innerKeySelector, EqualityComparer<TKey>.Default);
    }

    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <param name="resultSelector">A function used to combine the joined sequences</param>
    /// <param name="comparer">The equality comparer used to compare keys</param>
    /// <remarks>The provided equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector,
        IEqualityComparer<TKey> comparer) {

        if (resultSelector == null) {
            throw new ArgumentNullException(nameof(resultSelector));
        }

        return outer
            .Join(inner, outerKeySelector, innerKeySelector, comparer)
            .Select(pair => resultSelector(pair.outer, pair.inner));
    }

    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <param name="resultSelector">A function used to combine the joined sequences</param>
    /// <remarks>The default equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector) {
        
        if (resultSelector == null) {
            throw new ArgumentNullException(nameof(resultSelector));
        }

        return outer
            .Join(inner, outerKeySelector, innerKeySelector)
            .Select(pair => resultSelector(pair.outer, pair.inner));
    }
    
    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <param name="comparer">The equality comparer used to compare keys</param>
    /// <remarks>The provided equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<(TOuter outer, TInner inner)> Join<TOuter, TInner, TKey>(
        this IAsyncEnumerable<TOuter> outer,
        IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        IEqualityComparer<TKey> comparer) {

        return outer.Join(inner.ToAsyncEnumerable(), outerKeySelector, innerKeySelector, comparer);
    }
    
    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <remarks>The default equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<(TOuter outer, TInner inner)> Join<TOuter, TInner, TKey>(
        this IAsyncEnumerable<TOuter> outer,
        IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector) {

        return outer.Join(inner.ToAsyncEnumerable(), outerKeySelector, innerKeySelector);
    }

    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <param name="resultSelector">A function used to combine the joined sequences</param>
    /// <param name="comparer">The equality comparer used to compare keys</param>
    /// <remarks>The provided equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector,
        IEqualityComparer<TKey> comparer) {

        return outer.Join(inner.ToAsyncEnumerable(), outerKeySelector, innerKeySelector, resultSelector, comparer);
    }

    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <param name="resultSelector">A function used to combine the joined sequences</param>
    /// <remarks>The default equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector) {
        
        return outer.Join(inner.ToAsyncEnumerable(), outerKeySelector, innerKeySelector, resultSelector);
    }
    
    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <param name="comparer">The equality comparer used to compare keys</param>
    /// <remarks>The provided equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<(TOuter outer, TInner inner)> Join<TOuter, TInner, TKey>(
        this IAsyncEnumerable<TOuter> outer,
        IObservable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        IEqualityComparer<TKey> comparer) {

        return outer.Join(inner.ToAsyncEnumerable(), outerKeySelector, innerKeySelector, comparer);
    }
    
    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <remarks>The default equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<(TOuter outer, TInner inner)> Join<TOuter, TInner, TKey>(
        this IAsyncEnumerable<TOuter> outer,
        IObservable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector) {

        return outer.Join(inner.ToAsyncEnumerable(), outerKeySelector, innerKeySelector);
    }

    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <param name="resultSelector">A function used to combine the joined sequences</param>
    /// <param name="comparer">The equality comparer used to compare keys</param>
    /// <remarks>The provided equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IObservable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector,
        IEqualityComparer<TKey> comparer) {

        return outer.Join(inner.ToAsyncEnumerable(), outerKeySelector, innerKeySelector, resultSelector, comparer);
    }

    /// <summary>
    ///     Correlates and combines the elements in two sequences using matching keys. This is the equivalent of an inner join in SQL.
    /// </summary>
    /// <param name="outer">The first sequence to join</param>
    /// <param name="inner">The second sequence to join</param>
    /// <param name="outerKeySelector">The key selector for the first sequence</param>
    /// <param name="innerKeySelector">The key selector for the second sequence</param>
    /// <param name="resultSelector">A function used to combine the joined sequences</param>
    /// <remarks>The default equality comparer is used to compare keys</remarks>
    public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IObservable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector) {
        
        return outer.Join(inner.ToAsyncEnumerable(), outerKeySelector, innerKeySelector, resultSelector);
    }
}