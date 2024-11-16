using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> AsyncSelect<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, ValueTask<TResult>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        // Local function to be our SelectWhere selector
        async ValueTask<SelectWhereResult<TResult>> selectWhereFunc(TSource x) {
            return new(true, await selector(x));
        }

        // First try to compose this operation with a previous SelectWhere
        if (source is ISelectWhereTaskOperator<TSource> selectWhereOp) {
            return selectWhereOp.SelectWhereTask(selectWhereFunc);
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new SelectWhereTaskOperator<TSource, TResult>(pars, source, selectWhereFunc);
    }
}