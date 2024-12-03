using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {

    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        var totalComparer = new KeyEqualityComparer<TSource, TKey>(selector, comparer);

        return source.Intersect(second, totalComparer);
    }

    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> selector) {

        return source.IntersectBy(second, selector, EqualityComparer<TKey>.Default);
    }

    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        Func<TSource, TKey> selector) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }

    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        Func<TSource, TKey> selector) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }

    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Task<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Task<TSource> second,
        Func<TSource, TKey> selector) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }

    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        ValueTask<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    public static IAsyncEnumerable<TSource> IntersectBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        ValueTask<TSource> second,
        Func<TSource, TKey> selector) {

        return source.IntersectBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }
}