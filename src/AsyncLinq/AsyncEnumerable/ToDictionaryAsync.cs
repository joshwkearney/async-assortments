namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static ValueTask<Dictionary<TKey, TElement>> ToDictionaryAsync<TSource, TKey, TElement>(
        this IAsyncEnumerable<TSource> sequence,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        CancellationToken cancellationToken = default) where TKey : notnull {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        if (keySelector == null) {
            throw new ArgumentNullException(nameof(keySelector));
        }

        if (elementSelector == null) {
            throw new ArgumentNullException(nameof(elementSelector));
        }

        return ToDictionaryHelper(sequence, keySelector, elementSelector, cancellationToken);
    }

    private static async ValueTask<Dictionary<TKey, TElement>> ToDictionaryHelper<TSource, TKey, TElement>(
        this IAsyncEnumerable<TSource> sequence,
        Func<TSource, TKey> keySelector,
        Func<TSource, TElement> elementSelector,
        CancellationToken cancellationToken) where TKey : notnull {

        var dict = new Dictionary<TKey, TElement>();

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            dict.Add(keySelector(item), elementSelector(item));
        }

        return dict;
    }
}