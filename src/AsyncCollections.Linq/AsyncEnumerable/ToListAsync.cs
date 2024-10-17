namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> sequence) {
        var list = new List<T>();

        await foreach (var item in sequence) {
            list.Add(item);
        }

        return list;
    }
}