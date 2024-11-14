using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> AsyncWhere<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        // Local function to be our SelectWhere selector
        async ValueTask<SelectWhereResult<TSource>> selector(TSource x) {
            return new(await predicate(x), x);
        }

        // First try to compose this operation with a previous SelectWhere
        if (source is IAsyncSelectWhereOperator<TSource> selectWhereOp) {
            return selectWhereOp.ComposeWith(selector);
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new AsyncSelectWhereOperator<TSource, TSource>(source, selector, pars);
    }
}