/*using System.Threading.Channels;
using AsyncLinq.Operators;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector) where TKey : notnull {

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

        var pars = new AsyncOperatorParams();

        if (outer is IAsyncOperator<TOuter> op) {
            pars = op.Params;
        }

        return new JoinOperator<TOuter, TInner, TKey, TResult>(
            pars,
            outer,
            inner,
            outerKeySelector,
            innerKeySelector,
            resultSelector);
    }
}*/