using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> AsyncWhere<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<bool>> predicate) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        // Local function to be our SelectWhere selector
        async ValueTask<SelectWhereResult<TSource>> selector(TSource x, CancellationToken token) {
            return new(await predicate(x, token), x);
        }

        // First try to compose this operation with a previous SelectWhere
        if (source is ISelectWhereTaskOperator<TSource> selectWhereOp) {
            return selectWhereOp.SelectWhereTask(selector);
        }

        var pars = source.GetPipelineExecution();

        return new SelectWhereTaskOperator<TSource, TSource>(pars, source, selector);
    }
    
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
        async ValueTask<SelectWhereResult<TSource>> selector(TSource x, CancellationToken _) {
            return new(await predicate(x), x);
        }

        // First try to compose this operation with a previous SelectWhere
        if (source is ISelectWhereTaskOperator<TSource> selectWhereOp) {
            return selectWhereOp.SelectWhereTask(selector);
        }

        var pars = source.GetPipelineExecution();

        return new SelectWhereTaskOperator<TSource, TSource>(pars, source, selector);
    }
}