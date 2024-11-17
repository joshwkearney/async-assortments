namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static ValueTask<TSource?> LastOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return Helper();

        async ValueTask<TSource?> Helper() {
            var last = default(TSource);

            await foreach (var item in source.WithCancellation(cancellationToken)) {
                last = item;
            }

            return last;
        }
    }
    
    public static ValueTask<TSource> LastOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        TSource defaultValue,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return Helper();

        async ValueTask<TSource> Helper() {
            var last = defaultValue;

            await foreach (var item in source.WithCancellation(cancellationToken)) {
                last = item;
            }

            return last;
        }
    }
    
    public static ValueTask<TSource?> LastOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return source.Where(predicate).LastOrDefaultAsync(cancellationToken);
    }
    
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