using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {

    public static IAsyncEnumerable<TSource> Except<TSource>(
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

        return new ExceptOperator<TSource>(
            source.GetScheduleMode(),
            source,
            second,
            comparer);
    }

    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TSource> second) {

        return source.Except(second, EqualityComparer<TSource>.Default);
    }

    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Except(second.ToAsyncEnumerable(), comparer);
    }

    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second) {

        return source.Except(second.ToAsyncEnumerable(), EqualityComparer<TSource>.Default);
    }

    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Except(second.ToAsyncEnumerable(), comparer);
    }

    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second) {

        return source.Except(second.ToAsyncEnumerable(), EqualityComparer<TSource>.Default);
    }

    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        Task<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Except(second.ToAsyncEnumerable(), comparer);
    }

    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        Task<TSource> second) {

        return source.Except(second.ToAsyncEnumerable(), EqualityComparer<TSource>.Default);
    }

    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        ValueTask<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Except(second.ToAsyncEnumerable(), comparer);
    }

    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        ValueTask<TSource> second) {

        return source.Except(second.ToAsyncEnumerable(), EqualityComparer<TSource>.Default);
    }
}