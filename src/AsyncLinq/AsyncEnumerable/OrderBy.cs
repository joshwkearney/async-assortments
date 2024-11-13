using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static async IAsyncEnumerable<TSource> Order<TSource>(
        this IAsyncEnumerable<TSource> source, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var list = await source.ToListAsync(cancellationToken);

        foreach (var item in list.OrderBy(x => x)) {
            yield return item;
        }
    }

    public static async IAsyncEnumerable<TSource> OrderBy<TSource, TKey>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var list = await source.ToListAsync(cancellationToken);

        foreach (var item in list.OrderBy(keySelector)) {
            yield return item;
        }
    }

    public static async IAsyncEnumerable<TSource> OrderDescending<TSource>(
        this IAsyncEnumerable<TSource> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var list = await source.ToListAsync(cancellationToken);

        foreach (var item in list.OrderByDescending(x => x)) {
            yield return item;
        }
    }

    public static async IAsyncEnumerable<TResult> OrderByDescending<TResult, TKey>(
        this IAsyncEnumerable<TResult> source,
        Func<TResult, TKey> keySelector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var list = await source.ToListAsync(cancellationToken);

        foreach (var item in list.OrderByDescending(keySelector)) {
            yield return item;
        }
    }
}