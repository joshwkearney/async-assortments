namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Returns the first element in the sequence, or the type's default value.
    ///     if the sequence is empty.
    /// </summary>
    /// <param name="source">The original sequence.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<TSource?> FirstOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return FirstOrDefaultHelper(source, cancellationToken);        
    }

    private static async ValueTask<TSource?> FirstOrDefaultHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken) {

        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
        var worked = await enumerator.MoveNextAsync();

        // We got the first element (or not), so cancel the rest of it
        tokenSource.Cancel();

        if (!worked) {
            return default;
        }

        return enumerator.Current;
    }
    
    /// <summary>
    ///     Returns the first element in the sequence, or a provided default value if
    ///     the sequence is empty.
    /// </summary>
    /// <param name="source">The original sequence.</param>
    /// <param name="defaultValue">The default value to return if the sequence is empty.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<TSource> FirstOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        TSource defaultValue,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return FirstOrDefaultHelper(source, defaultValue, cancellationToken);        
    }

    private static async ValueTask<TSource> FirstOrDefaultHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        TSource defaultValue,
        CancellationToken cancellationToken) {

        using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
        var worked = await enumerator.MoveNextAsync();

        // We got the first element (or not), so cancel the rest of it
        tokenSource.Cancel();

        if (!worked) {
            return defaultValue;
        }

        return enumerator.Current;
    }
    
    /// <summary>
    ///     Returns the first element in the sequence that matches a given predicate,
    ///     or the type's default value if the sequence is empty.
    /// </summary>
    /// <param name="source">The original sequence.</param>
    /// <param name="predicate">A function that determines which elements can be returned.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<TSource?> FirstOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        
        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        return source.Where(predicate).FirstOrDefaultAsync(cancellationToken);
    }
    
    /// <summary>
    ///     Returns the first element in the sequence that matches a given predicate,
    ///     or a provided default value if the sequence is empty.
    /// </summary>
    /// <param name="source">The original sequence.</param>
    /// <param name="predicate">A function that determines which elements can be returned.</param>
    /// <param name="defaultValue">The default value to return if the sequence is empty.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <exception cref="TaskCanceledException">
    ///     The enumeration was cancelled with the provided <see cref="CancellationToken" />.
    /// </exception>
    public static ValueTask<TSource> FirstOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        TSource defaultValue,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }
        
        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        return source.Where(predicate).FirstOrDefaultAsync(defaultValue, cancellationToken);
    }
}