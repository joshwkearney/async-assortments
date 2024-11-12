namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<Dictionary<TKey, TValue>> ToDictionaryAsync<T, TKey, TValue>(
        this IAsyncEnumerable<T> sequence,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector,
        CancellationToken cancellationToken = default) where TKey : notnull {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        if (keySelector == null) {
            throw new ArgumentNullException(nameof(keySelector));
        }

        if (valueSelector == null) {
            throw new ArgumentNullException(nameof(valueSelector));
        }

        var dict = new Dictionary<TKey, TValue>();

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            dict.Add(keySelector(item), valueSelector(item));
        }

        return dict;
    }
}