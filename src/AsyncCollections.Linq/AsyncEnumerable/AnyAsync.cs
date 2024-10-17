namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<bool> AnyAsync<T>(this IAsyncEnumerable<T> sequence) {
        await using var enumerator = sequence.GetAsyncEnumerator();

        return await enumerator.MoveNextAsync();
    }
}