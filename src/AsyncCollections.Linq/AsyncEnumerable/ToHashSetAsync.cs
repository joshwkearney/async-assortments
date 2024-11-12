namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<HashSet<T>> ToHashSetAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        var list = new HashSet<T>();

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            list.Add(item);
        }

        return list;
    }
}