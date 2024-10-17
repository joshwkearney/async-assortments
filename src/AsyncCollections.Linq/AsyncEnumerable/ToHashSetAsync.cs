namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<HashSet<T>> ToHashSetAsync<T>(this IAsyncEnumerable<T> sequence) {
        var list = new HashSet<T>();

        await foreach (var item in sequence) {
            list.Add(item);
        }

        return list;
    }
}