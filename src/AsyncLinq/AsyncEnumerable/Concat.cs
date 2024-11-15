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

        if (source == EmptyOperator<TSource>.Instance) {
            return second;
        }

        if (second == EmptyOperator<TSource>.Instance) {
            return source;
        }

        if (source is IConcatOperator<TSource> concatOp) {
            return concatOp.Concat(second);
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new ConcatOperator<TSource>([source, second], pars);
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

        var isSecondEmpty = object.ReferenceEquals(second, Array.Empty<TSource>()) ||
                            object.ReferenceEquals(second, Enumerable.Empty<TSource>());

        if (isSecondEmpty) {
            return source;
        }

        if (source is IConcatEnumerablesOperator<TSource> concatOp) {
            return concatOp.ConcatEnumerables([], second);
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new ConcatEnumerablesOperator<TSource>(source, [], second, pars);
    }
}