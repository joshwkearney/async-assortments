using AsyncLinq.Operators;
using System.Linq;

namespace AsyncLinq;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return new EnumerableOperator<TSource>(source);
    }    
}