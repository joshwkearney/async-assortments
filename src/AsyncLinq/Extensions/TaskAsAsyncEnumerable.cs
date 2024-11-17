
using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this Task<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return new SingletonOperator<TSource>(default, _ => new ValueTask<TSource>(source));
    }
}
