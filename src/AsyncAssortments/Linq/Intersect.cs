using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Produces a set intersection between two sequences
    /// </summary>
    /// <remarks>Uses the provided equality comparer to compare elements</remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="comparer">The equality comparer used to compare elements</param>
    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        
        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }
        
        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }
        
        var resultOp = new JoinOperator<TSource, TSource, TSource>(
            source.GetScheduleMode(),
            source,
            second,
            x => x,
            x => x,
            comparer);

        return resultOp
            .Select(pair => pair.first)
            .Distinct(comparer);
    }

    /// <summary>
    ///     Produces a set intersection between two sequences
    /// </summary>
    /// <remarks>Uses the default equality comparer to compare elements</remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TSource> second) {

        return source.Intersect(second, EqualityComparer<TSource>.Default);
    }

    /// <summary>
    ///     Produces a set intersection between two sequences
    /// </summary>
    /// <remarks>Uses the provided equality comparer to compare elements</remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="comparer">The equality comparer used to compare elements</param>
    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Intersect(second.ToAsyncEnumerable(), comparer);
    }

    /// <summary>
    ///     Produces a set intersection between two sequences
    /// </summary>
    /// <remarks>Uses the default equality comparer to compare elements</remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second) {

        return source.Intersect(second.ToAsyncEnumerable());
    }

    /// <summary>
    ///     Produces a set intersection between two sequences
    /// </summary>
    /// <remarks>Uses the provided equality comparer to compare elements</remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="comparer">The equality comparer used to compare elements</param>
    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Intersect(second.ToAsyncEnumerable(), comparer);
    }

    /// <summary>
    ///     Produces a set intersection between two sequences
    /// </summary>
    /// <remarks>Uses the default equality comparer to compare elements</remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second) {

        return source.Intersect(second.ToAsyncEnumerable());
    }
}