namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<T> LastAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

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