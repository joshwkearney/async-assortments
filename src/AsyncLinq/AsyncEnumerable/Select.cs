using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>
    /// Projects each element of a sequence into a new form.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <c>source</c>.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <c>selector</c>.</typeparam>
    /// <param name="source">A sequence of values to invoke a transform function on.</param>
    /// <param name="selector">A transform function to apply to each source element.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}" /> whose elements are the result of invoking the transform function on each element of source.</returns>
    /// <exception cref="ArgumentNullException"><c>source</c> or <c>selector</c> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, TResult> selector) {

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

        return new SelectOperator<TSource, TResult>(source, selector, pars);
    }

    private class SelectOperator<T, E> : IAsyncOperator<E> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly Func<T, E> selector;

        public AsyncOperatorParams Params { get; }

        public SelectOperator(IAsyncEnumerable<T> collection, Func<T, E> selector, AsyncOperatorParams pars) {
            this.parent = collection;
            this.selector = selector;
            this.Params = pars;
        }

        public async IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                yield return this.selector(item);
            }
        }
    }
}