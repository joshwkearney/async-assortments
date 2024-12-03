using System.Threading.Channels;
using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector,
        IEqualityComparer<TKey> comparer) {

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

        if (outer is IAsyncOperator<TOuter> op) {
            outer = op.WithScheduleMode(op.ScheduleMode.MakeUnordered());
        }

        var resultOp = new JoinOperator<TOuter, TInner, TKey>(
            outer.GetScheduleMode(),
            outer,
            inner,
            outerKeySelector,
            innerKeySelector,
            comparer);

        return resultOp.Select(pair => resultSelector(pair.first, pair.second));
    }

    public static IAsyncEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(
        this IAsyncEnumerable<TOuter> outer,
        IAsyncEnumerable<TInner> inner,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector) {

        return outer.Join(
            inner,
            outerKeySelector,
            innerKeySelector,
            resultSelector,
            EqualityComparer<TKey>.Default);
    }
}