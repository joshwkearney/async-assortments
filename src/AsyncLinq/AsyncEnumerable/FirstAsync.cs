using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static ValueTask<TSource> FirstAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return Helper();
        
        async ValueTask<TSource> Helper() {
            using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
            var worked = await enumerator.MoveNextAsync();
            
            // We got the first element (or not), so cancel the rest of it
            tokenSource.Cancel();

            if (!worked) {
                throw new InvalidOperationException("Sequence contains no elements");
            }

            return enumerator.Current;
        }
    }
    
    public static ValueTask<TSource> FirstAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        return source.Where(predicate).FirstAsync(cancellationToken);
    }
}