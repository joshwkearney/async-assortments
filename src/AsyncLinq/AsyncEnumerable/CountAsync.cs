using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static ValueTask<int> CountAsync<TSource>(
        this IAsyncEnumerable<TSource> source, 
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is ICountOperator<TSource> op) {
            var count = op.Count();

            if (count >= 0) {
                return new ValueTask<int>(count);
            }
        }

        return Helper();

        async ValueTask<int> Helper() {
            int count = 0;

            await foreach (var item in source.WithCancellation(cancellationToken)) {
                count++;
            }

            return count;
        }
    }

    public static ValueTask<int> CountAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, bool> predicate,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return source.Where(predicate).CountAsync(cancellationToken);
    }
}