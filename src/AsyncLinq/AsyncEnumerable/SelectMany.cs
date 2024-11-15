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

        // TODO: Implement unordered select many

        //if (pars.IsUnordered) {
        //    return new UnorderedSelectManyAsyncOperator<TSource, TResult>(source, selector, pars);
        //}
        //else {
            return new SequentialSelectManyAsyncOperator<TSource, TResult>(source, selector, pars);
        //}
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

        return new EnumerableSelectManyAsyncOperator<TSource, TResult>(source, selector, pars);
    }

    private class SequentialSelectManyAsyncOperator<T, E> : IAsyncOperator<E> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly Func<T, IAsyncEnumerable<E>> selector;
        
        public AsyncOperatorParams Params { get; }

        public SequentialSelectManyAsyncOperator(
            IAsyncEnumerable<T> sequence,
            Func<T, IAsyncEnumerable<E>> selector,
            AsyncOperatorParams pars) {

            this.parent = sequence;
            this.selector = selector;
            this.Params = pars;
        }

        public async IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                await foreach (var sub in selector(item).WithCancellation(cancellationToken)) {
                    yield return sub;
                }
            }
        }
    }

    private class EnumerableSelectManyAsyncOperator<T, E> : IAsyncOperator<E> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly Func<T, IEnumerable<E>> selector;

        public AsyncOperatorParams Params { get; }

        public EnumerableSelectManyAsyncOperator(
            IAsyncEnumerable<T> sequence,
            Func<T, IEnumerable<E>> selector,
            AsyncOperatorParams pars) {

            this.parent = sequence;
            this.selector = selector;
            this.Params = pars;
        }

        public async IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                foreach (var sub in selector(item)) {
                    yield return sub;
                }
            }
        }
    }
}