namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<T?> LastOrDefaultAsync<T>(this IAsyncEnumerable<T> sequence) {
        var last = default(T);

        await foreach (var item in sequence) {
            last = item;
        }

        return last;
    }
}