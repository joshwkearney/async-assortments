using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {

    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        if (source is IAsyncOperator<TSource> op) {
            source = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        return source.Concat(second).Distinct(comparer);
    }

    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TSource> second) {

        return source.Union(second, EqualityComparer<TSource>.Default);
    }

    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        if (source is IAsyncOperator<TSource> op) {
            source = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        return source.Concat(second).Distinct(comparer);
    }

    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second) {

        return source.Union(second, EqualityComparer<TSource>.Default);
    }

    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        if (source is IAsyncOperator<TSource> op) {
            source = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        return source.Concat(second).Distinct(comparer);
    }

    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second) {

        return source.Union(second, EqualityComparer<TSource>.Default);
    }

    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        Task<TSource> second,
        IEqualityComparer<TSource> comparer) {

        if (source is IAsyncOperator<TSource> op) {
            source = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        return source.Concat(second).Distinct(comparer);
    }

    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        Task<TSource> second) {

        return source.Union(second, EqualityComparer<TSource>.Default);
    }

    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        ValueTask<TSource> second,
        IEqualityComparer<TSource> comparer) {

        if (source is IAsyncOperator<TSource> op) {
            source = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        return source.Concat(second).Distinct(comparer);
    }

    public static IAsyncEnumerable<TSource> Union<TSource>(
        this IAsyncEnumerable<TSource> source,
        ValueTask<TSource> second) {

        return source.Union(second, EqualityComparer<TSource>.Default);
    }
}