using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Where<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, bool> predicate) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        // Try to compose with a previous operator
        if (source is IWhereOperator<TSource> whereOp) {
            return whereOp.Where(predicate);
        }

        var pars = source.GetPipelineExecution();

        return new SelectWhereOperator<TSource, TSource>(pars, source, x => new(predicate(x), x));
    }
}