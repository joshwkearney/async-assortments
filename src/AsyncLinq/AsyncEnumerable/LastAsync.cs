namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static ValueTask<TSource> LastAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return Helper();

        async ValueTask<TSource> Helper() {
            await using var iterator = source.GetAsyncEnumerator(cancellationToken);

            if (!await iterator.MoveNextAsync()) {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            var last = iterator.Current;

            while (await iterator.MoveNextAsync()) {
                last = iterator.Current;
            }

            return last;
        }
    }
    
    public static ValueTask<TSource> LastAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        return source.Where(predicate).LastAsync(cancellationToken);
    }
}