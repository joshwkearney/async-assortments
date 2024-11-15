using System.Collections;
using AsyncLinq.Operators;
using System.Linq;

namespace AsyncLinq;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        // No need to allocate extra objects if we don't need to
        if (object.ReferenceEquals(source, Array.Empty<TSource>()) || object.ReferenceEquals(source, Enumerable.Empty<TSource>())) {
            return EmptyOperator<TSource>.Instance;
        }

        return new EnumerableOperator<TSource>(source);
    }    
}