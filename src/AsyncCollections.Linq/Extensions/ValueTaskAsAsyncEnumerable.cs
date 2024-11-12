
namespace AsyncCollections.Linq;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this ValueTask<T> task) {
        // We need to do this because you can call .GetEnumerator() multiple times on a sequence
        return task.AsTask().AsAsyncEnumerable();
    }
}
