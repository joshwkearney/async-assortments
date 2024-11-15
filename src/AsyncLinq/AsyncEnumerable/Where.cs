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

        // This is our SelectWhere selector
        SelectWhereResult<TSource> selectWhereFunc(TSource item) => new(predicate(item), item);

        // Try to compose with a previous operator
        if (source is ISelectWhereOperator<TSource> selectWhereOp) {
            return selectWhereOp.SelectWhere(selectWhereFunc);
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new SelectWhereOperator<TSource, TSource>(source, selectWhereFunc, pars);
    }
}