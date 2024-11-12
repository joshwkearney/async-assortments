namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static async ValueTask<bool> AnyAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        await using var enumerator = source.GetAsyncEnumerator(cancellationToken);

        return await enumerator.MoveNextAsync();
    }

    public static ValueTask<bool> AnyAsync<TSource>(
       this IAsyncEnumerable<TSource> source,
       Func<TSource, bool> predicate,
       CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        return source.Where(predicate).AnyAsync(cancellationToken);
    }
}