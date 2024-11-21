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

        // First try to compose this operation with a previous operator
        if (source is IAsyncWhereOperator<TSource> whereOp) {
            return whereOp.AsyncWhere(predicate);
        }

        var pars = source.GetPipelineExecution();

        return new SelectWhereTaskOperator<TSource, TSource>(pars, source, async (x, c) => new(await predicate(x, c), x));
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

        return source.AsyncWhere((x, c) => predicate(x));
    }
}