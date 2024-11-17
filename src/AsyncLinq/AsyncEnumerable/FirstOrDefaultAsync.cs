using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static ValueTask<TSource?> FirstOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return Helper();

        async ValueTask<TSource?> Helper() {
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
    }
    
    public static ValueTask<TSource> FirstOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        TSource defaultValue,
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
                return defaultValue;
            }

            return enumerator.Current;
        }
    }
    
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