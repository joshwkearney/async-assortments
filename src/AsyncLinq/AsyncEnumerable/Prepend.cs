using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Prepend<TSource>(
        this IAsyncEnumerable<TSource> source, 
        TSource element) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IEnumerableConcatOperator<TSource> concatOp) {
            return concatOp.ComposeWith([element], []);
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new Operators.EnumerableConcatOperator<TSource>(source, [element], [], pars);
    }
}