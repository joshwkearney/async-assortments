using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {

    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Join(second, x => x, x => x, (x, y) => x, comparer).Distinct(comparer);
    }

    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TSource> second) {

        return source.Join(second, x => x, x => x, (x, y) => x).Distinct();
    }

    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Intersect(second.ToAsyncEnumerable(), comparer);
    }

    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second) {

        return source.Intersect(second.ToAsyncEnumerable());
    }

    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Intersect(second.ToAsyncEnumerable(), comparer);
    }

    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second) {

        return source.Intersect(second.ToAsyncEnumerable());
    }

    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        Task<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Intersect(second.ToAsyncEnumerable(), comparer);

    }

    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        Task<TSource> second) {

        return source.Intersect(second.ToAsyncEnumerable());
    }

    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        ValueTask<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Intersect(second.ToAsyncEnumerable(), comparer);
    }

    public static IAsyncEnumerable<TSource> Intersect<TSource>(
        this IAsyncEnumerable<TSource> source,
        ValueTask<TSource> second) {

        return source.Intersect(second.ToAsyncEnumerable());
    }
}