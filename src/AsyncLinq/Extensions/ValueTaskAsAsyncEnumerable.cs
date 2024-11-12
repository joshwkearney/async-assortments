
namespace AsyncLinq;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this ValueTask<TSource> source) {
        // We need to do this because you can call .GetEnumerator() multiple times on a sequence
        return source.AsTask().AsAsyncEnumerable();
    }
}
