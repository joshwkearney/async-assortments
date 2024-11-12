
namespace AsyncCollections.Linq;

public static partial class AsyncEnumerableExtensions {
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this Task<T> task) {
        if (task == null) {
            throw new ArgumentNullException(nameof(task));
        }

        yield return await task;
    }
}
