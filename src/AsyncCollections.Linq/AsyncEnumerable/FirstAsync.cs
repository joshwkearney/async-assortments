namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<T> FirstAsync<T>(this IAsyncEnumerable<T> sequence) {
        await using var enumerator = sequence.GetAsyncEnumerator();
        var worked = await enumerator.MoveNextAsync();

        if (!worked) {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        return enumerator.Current;
    }
}