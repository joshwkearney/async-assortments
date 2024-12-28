using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Produces a set union between two sequences
    /// </summary>
    /// <remarks>Uses the provided equality comparer to compare elements</remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="comparer">The equality comparer used to compare elements</param>
    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        if (source is IAsyncOperator<TSource> op) {
            source = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        return source.Concat(second).Distinct(comparer);
    }

    /// <summary>
    ///     Produces a set union between two sequences
    /// </summary>
    /// <remarks>Uses the provided equality comparer to compare elements</remarks>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TSource> second) {

        return source.Union(second, EqualityComparer<TSource>.Default);
    }
    
    /// <inheritdoc cref="Union{TSource}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSource},IEqualityComparer{TSource})" />
    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        if (source is IAsyncOperator<TSource> op) {
            source = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        return source.Concat(second).Distinct(comparer);
    }

    /// <inheritdoc cref="Union{TSource}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSource})" />
    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second) {

        return source.Union(second, EqualityComparer<TSource>.Default);
    }


    /// <inheritdoc cref="Union{TSource}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSource},IEqualityComparer{TSource})" />
    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        if (source is IAsyncOperator<TSource> op) {
            source = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        return source.Concat(second).Distinct(comparer);
    }

    /// <inheritdoc cref="Union{TSource}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSource})" />
    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second) {

        return source.Union(second, EqualityComparer<TSource>.Default);
    }
}