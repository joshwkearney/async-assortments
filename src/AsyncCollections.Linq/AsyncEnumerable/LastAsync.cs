namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<T> LastAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) {

        var isEmpty = true;
        var last = default(T);

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            last = item;
            isEmpty = false;
        }

        if (isEmpty) {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        return last!;
    }
}