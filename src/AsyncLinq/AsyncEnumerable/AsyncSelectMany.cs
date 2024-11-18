using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    //public static IAsyncEnumerable<TResult> AsyncSelectMany<TSource, TResult>(
    //    this IAsyncEnumerable<TSource> source, 
    //    Func<TSource, ValueTask<IAsyncEnumerable<TResult>>> selector) {

    //    if (source == null) {
    //        throw new ArgumentNullException(nameof(source));
    //    }

    //    if (selector == null) {
    //        throw new ArgumentNullException(nameof(selector));
    //    }

    //    var pars = new AsyncOperatorParams();

    //    if (source is IAsyncOperator<TSource> op) {
    //        pars = op.Params;
    //    }

    //    return new FlattenOperator<TResult>(pars, source.AsyncSelect(selector));
    //}

    //public static IAsyncEnumerable<TResult> AsyncSelectMany<TSource, TResult>(
    //    this IAsyncEnumerable<TSource> source, 
    //    Func<TSource, CancellationToken, ValueTask<IAsyncEnumerable<TResult>>> selector) {

    //    if (source == null) {
    //        throw new ArgumentNullException(nameof(source));
    //    }

    //    if (selector == null) {
    //        throw new ArgumentNullException(nameof(selector));
    //    }

    //    var pars = new AsyncOperatorParams();

    //    if (source is IAsyncOperator<TSource> op) {
    //        pars = op.Params;
    //    }

    //    return new FlattenOperator<TResult>(pars, source.AsyncSelect(selector));
    //}

    //public static IAsyncEnumerable<TResult> AsyncSelectMany<TSource, TResult>(
    //    this IAsyncEnumerable<TSource> source,
    //    Func<TSource, ValueTask<IEnumerable<TResult>>> selector) {

    //    if (source == null) {
    //        throw new ArgumentNullException(nameof(source));
    //    }

    //    if (selector == null) {
    //        throw new ArgumentNullException(nameof(selector));
    //    }

    //    var pars = new AsyncOperatorParams();

    //    if (source is IAsyncOperator<TSource> op) {
    //        pars = op.Params;
    //    }

    //    return new FlattenEnumerablesOperator<TResult>(pars, source.AsyncSelect(selector));
    //}

    //public static IAsyncEnumerable<TResult> AsyncSelectMany<TSource, TResult, G>(
    //    this IAsyncEnumerable<TSource> source,
    //    Func<TSource, CancellationToken, ValueTask<IEnumerable<TResult>>> selector) {

    //    if (source == null) {
    //        throw new ArgumentNullException(nameof(source));
    //    }

    //    if (selector == null) {
    //        throw new ArgumentNullException(nameof(selector));
    //    }

    //    var pars = new AsyncOperatorParams();

    //    if (source is IAsyncOperator<TSource> op) {
    //        pars = op.Params;
    //    }

    //    return new FlattenEnumerablesOperator<TResult>(pars, source.AsyncSelect(selector));
    //}
}