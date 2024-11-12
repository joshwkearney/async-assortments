
namespace AsyncCollections.Linq;

public static partial class AsyncEnumerableExtensions {
    public static async IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this Task<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        yield return await source;
    }
}
