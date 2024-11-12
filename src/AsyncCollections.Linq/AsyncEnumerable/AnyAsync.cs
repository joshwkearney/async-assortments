namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<bool> AnyAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        await using var enumerator = sequence.GetAsyncEnumerator(cancellationToken);

        return await enumerator.MoveNextAsync();
    }
}