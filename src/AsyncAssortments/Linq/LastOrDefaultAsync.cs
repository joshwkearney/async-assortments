namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Returns the last element in the sequence, or the type's default value.
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
    public static ValueTask<TSource?> LastOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return LastOrDefaultHelper(source, cancellationToken);        
    }

    private static async ValueTask<TSource?> LastOrDefaultHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken) {

        var last = default(TSource);

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            last = item;
        }

        return last;
    }
    
    /// <summary>
    ///     Returns the last element in the sequence, or a provided default value if
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
    public static ValueTask<TSource> LastOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        TSource defaultValue,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return LastOrDefaultHelper(source, defaultValue, cancellationToken);        
    }

    private static async ValueTask<TSource> LastOrDefaultHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        TSource defaultValue,
        CancellationToken cancellationToken) {

        var last = defaultValue;

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            last = item;
        }

        return last;
    }
    
    /// <summary>
    ///     Returns the last element in the sequence that matches a given predicate,
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
    public static ValueTask<TSource?> LastOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return source.Where(predicate).LastOrDefaultAsync(cancellationToken);
    }
    
    /// <summary>
    ///     Returns the last element in the sequence that matches a given predicate,
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
    public static ValueTask<TSource> LastOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        TSource defaultValue,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return source.Where(predicate).LastOrDefaultAsync(defaultValue, cancellationToken);
    }
}