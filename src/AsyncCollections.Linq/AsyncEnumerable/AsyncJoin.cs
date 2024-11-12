using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> AsyncJoin<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, ValueTask<TResult>> resultSelector) where TKey : notnull {

        if (outer == null) {
            throw new ArgumentNullException(nameof(outer));
        }

        if (inner == null) {
            throw new ArgumentNullException(nameof(inner));
        }

        if (outerKeySelector == null) {
            throw new ArgumentNullException(nameof(outerKeySelector));
        }

        if (innerKeySelector == null) {
            throw new ArgumentNullException(nameof(innerKeySelector));
        }

        if (resultSelector == null) {
            throw new ArgumentNullException(nameof(resultSelector));
        }

        return outer
            .Join(inner, outerKeySelector, innerKeySelector, resultSelector)
            .AsyncSelect(x => x);
    }
}