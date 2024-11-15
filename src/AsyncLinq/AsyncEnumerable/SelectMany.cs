using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, IAsyncEnumerable<TResult>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new FlattenOperator<TResult>(pars, source.Select(selector));
    }

    public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, IEnumerable<TResult>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new FlattenEnumerablesOperator<TResult>(pars, source.Select(selector));
    }
}