using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {

    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second,
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

        var totalComparer = new KeyEqualityComparer<TSource, TKey>(selector, comparer);

        return new ExceptOperator<TSource>(
            source.GetScheduleMode(),
            source,
            second,
            totalComparer);
    }

    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TSource> second,
        Func<TSource, TKey> selector) {

        return source.ExceptBy(second, selector, EqualityComparer<TKey>.Default);
    }

    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        Func<TSource, TKey> selector) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }

    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        Func<TSource, TKey> selector) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }

    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Task<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Task<TSource> second,
        Func<TSource, TKey> selector) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }

    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        ValueTask<TSource> second,
        Func<TSource, TKey> selector,
        IEqualityComparer<TKey> comparer) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, comparer);
    }

    public static IAsyncEnumerable<TSource> ExceptBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        ValueTask<TSource> second,
        Func<TSource, TKey> selector) {

        return source.ExceptBy(second.ToAsyncEnumerable(), selector, EqualityComparer<TKey>.Default);
    }
}