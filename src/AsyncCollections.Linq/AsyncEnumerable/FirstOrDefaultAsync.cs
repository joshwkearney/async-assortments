namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> sequence) {
        await using var enumerator = sequence.GetAsyncEnumerator();
        var worked = await enumerator.MoveNextAsync();

        if (!worked) {
            return default;
        }

        return enumerator.Current;
    }
}