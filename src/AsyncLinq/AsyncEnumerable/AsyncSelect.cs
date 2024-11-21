using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> AsyncSelect<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, CancellationToken, ValueTask<TResult>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        // First try to compose this operation with a previous operator
        if (source is IAsyncSelectOperator<TSource> selectOp) {
            return selectOp.AsyncSelect(selector);
        }

        var pars = source.GetPipelineExecution();

        return new SelectWhereTaskOperator<TSource, TResult>(
            pars, 
            source, 
            async (x, c) => new(true, await selector(x, c)));
    }
    
    public static IAsyncEnumerable<TResult> AsyncSelect<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, ValueTask<TResult>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        return source.AsyncSelect((x, _) => selector(x));
    }
}