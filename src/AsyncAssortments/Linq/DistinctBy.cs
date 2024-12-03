using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey> comparer) {

        var totalComparer = new KeyEqualityComparer<TSource, TKey>(keySelector, comparer);

        return source.Distinct(totalComparer);
    }

    public static IAsyncEnumerable<TSource> DistinctBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector) {

        var totalComparer = new KeyEqualityComparer<TSource, TKey>(keySelector, EqualityComparer<TKey>.Default);

        return source.Distinct(totalComparer);
    }
}