namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<T> FirstAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        await using var enumerator = sequence.GetAsyncEnumerator(cancellationToken);
        var worked = await enumerator.MoveNextAsync();

        if (!worked) {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        return enumerator.Current;
    }
}