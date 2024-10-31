namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<T?> LastOrDefaultAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) {

        var last = default(T);

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            last = item;
        }

        return last;
    }
}