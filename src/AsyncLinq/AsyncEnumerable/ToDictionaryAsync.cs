namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Collects a sequence into a <see cref="Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="keySelector">A function that selects the key for the dictionary.</param>
    /// <param name="elementSelector">A function that selects the value for the dictionary.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> that completes when the sequence has been 
    ///     asynchronously enumerated and collected into an dictionary using the provided
    ///     key and value selector.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
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