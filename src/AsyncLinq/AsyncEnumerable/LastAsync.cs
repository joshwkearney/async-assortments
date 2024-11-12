namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static async ValueTask<TSource> LastAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var isEmpty = true;
        var last = default(TSource);

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            last = item;
            isEmpty = false;
        }

        if (isEmpty) {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        return last!;
    }
}