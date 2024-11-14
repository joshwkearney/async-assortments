using System.Threading.Channels;
using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Concat<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }

        if (source is IConcatManyOperator<TSource> concatOp) {
            return concatOp.ComposeWith([second]);
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new ConcatManyOperator<TSource>([source, second], pars);
    }

    public static IAsyncEnumerable<TSource> Concat<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IEnumerable<TSource> second) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }

        if (source is IEnumerableConcatOperator<TSource> concatOp) {
            return concatOp.ComposeWith([], second);
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new EnumerableConcatOperator<TSource>(source, [], second, pars);
    }
}