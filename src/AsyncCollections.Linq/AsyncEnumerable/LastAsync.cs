namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<T> LastAsync<T>(this IAsyncEnumerable<T> sequence) {
        var empty = true;
        var last = default(T);

        await foreach (var item in sequence) {
            last = item;
            empty = false;
        }

        if (empty) {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        return last!;
    }
}