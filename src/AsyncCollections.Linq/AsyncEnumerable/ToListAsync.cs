using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<List<T>> ToListAsync<T>(
        this IAsyncEnumerable<T> sequence, 
        CancellationToken cancellationToken = default) {

        var list = new List<T>();

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            list.Add(item);
        }

        return list;
    }
}