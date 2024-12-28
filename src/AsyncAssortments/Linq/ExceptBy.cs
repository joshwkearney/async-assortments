using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Produces a set difference between two sequences according to a specified key selector function
    /// </summary>
    /// <remarks>
    ///     Elements are yielded from the source sequence if their associated keys do not appear in the second sequence
    /// </remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="selector">A selector that extracts a key from each element</param>
    /// <param name="comparer">A comparer used to compare keys</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
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
        
        if (source is IAsyncOperator<TSource> op) {
            source = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        return new ExceptByOperator<TSource, TKey>(
            source.GetScheduleMode(),
            source,
            second,
            selector,
            comparer);
    }

    /// <summary>
    ///     Produces a set difference between two sequences according to a specified key selector function
    /// </summary>
    /// <remarks>
    ///     Elements are yielded from the source sequence if their associated keys do not appear in the second sequence
    /// </remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="selector">A selector that extracts a key from each element</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TKey> second,
        Func<TSource, TKey> selector) {

        return source.ExceptBy(second, selector, EqualityComparer<TKey>.Default);
    }

    /// <summary>
    ///     Produces a set difference between two sequences according to a specified key selector function
    /// </summary>
    /// <remarks>
    ///     Elements are yielded from the source sequence if their associated keys do not appear in the second sequence
    /// </remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="selector">A selector that extracts a key from each element</param>
    /// <param name="comparer">A comparer used to compare keys</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TKey> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer)  {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    /// <summary>
    ///     Produces a set difference between two sequences according to a specified key selector function
    /// </summary>
    /// <remarks>
    ///     Elements are yielded from the source sequence if their associated keys do not appear in the second sequence
    /// </remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="selector">A selector that extracts a key from each element</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TKey> second,
        Func<TSource, TKey> selector) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }

    /// <summary>
    ///     Produces a set difference between two sequences according to a specified key selector function
    /// </summary>
    /// <remarks>
    ///     Elements are yielded from the source sequence if their associated keys do not appear in the second sequence
    /// </remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="selector">A selector that extracts a key from each element</param>
    /// <param name="comparer">A comparer used to compare keys</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TKey> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    /// <summary>
    ///     Produces a set difference between two sequences according to a specified key selector function
    /// </summary>
    /// <remarks>
    ///     Elements are yielded from the source sequence if their associated keys do not appear in the second sequence
    /// </remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="selector">A selector that extracts a key from each element</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TKey> second,
        Func<TSource, TKey> selector) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }
}