namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<T?> FirstOrDefaultAsync<T>(
        this IAsyncEnumerable<T> sequence,
        CancellationToken cancellationToken = default) {

        await using var enumerator = sequence.GetAsyncEnumerator(cancellationToken);
        var worked = await enumerator.MoveNextAsync();

        if (!worked) {
            return default;
        }

        return enumerator.Current;
    }
}