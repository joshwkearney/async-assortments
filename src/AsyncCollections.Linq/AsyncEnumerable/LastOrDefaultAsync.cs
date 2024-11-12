namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<TSource?> LastOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var last = default(TSource);

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            last = item;
        }

        return last;
    }
}