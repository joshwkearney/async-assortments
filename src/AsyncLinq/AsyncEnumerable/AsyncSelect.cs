using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> AsyncSelect<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, ValueTask<TResult>> selector) {

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

        if (pars.ExecutionMode == AsyncExecutionMode.Sequential) {
            return new SequentialAsyncSelectOperator<TSource, TResult>(source, selector, pars);
        }
        else if (pars.IsUnordered) {
            return new UnorderedAsyncSelectOperator<TSource, TResult>(source, selector, pars);
        }
        else {
            return new OrderedAsyncSelectOperator<TSource, TResult>(source, selector, pars);
        }
    }

    private class SequentialAsyncSelectOperator<T, E> : IAsyncOperator<E> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly Func<T, ValueTask<E>> selector;
        
        public AsyncOperatorParams Params { get; }

        public SequentialAsyncSelectOperator(
            IAsyncEnumerable<T> collection, 
            Func<T, ValueTask<E>> selector, 
            AsyncOperatorParams pars) {

            this.parent = collection;
            this.selector = selector;
            this.Params = pars;
        }

        public async IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                yield return await this.selector(item);
            }
        }
    }

    private class UnorderedAsyncSelectOperator<T, E> : IAsyncOperator<E> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly Func<T, ValueTask<E>> selector;

        public AsyncOperatorParams Params { get; }

        public UnorderedAsyncSelectOperator(
            IAsyncEnumerable<T> collection,
            Func<T, ValueTask<E>> selector,
            AsyncOperatorParams pars) {

            this.parent = collection;
            this.selector = selector;
            this.Params = pars;

            Debug.Assert(this.Params.ExecutionMode != AsyncExecutionMode.Sequential);
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return ParallelHelper.DoUnordered<T, E>(
                this.parent,
                async (x, channel) => {
                    await channel.Writer.WriteAsync(await this.selector(x), cancellationToken);
                },
                this.Params.ExecutionMode == AsyncExecutionMode.Parallel,
                cancellationToken);
        }
    }

    private class OrderedAsyncSelectOperator<T, E> : IAsyncOperator<E> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly Func<T, ValueTask<E>> selector;

        public AsyncOperatorParams Params { get; }

        public OrderedAsyncSelectOperator(
            IAsyncEnumerable<T> collection,
            Func<T, ValueTask<E>> selector,
            AsyncOperatorParams pars) {

            this.parent = collection;
            this.selector = selector;
            this.Params = pars;

            Debug.Assert(this.Params.ExecutionMode != AsyncExecutionMode.Sequential);
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            var isParallel = this.Params.ExecutionMode == AsyncExecutionMode.Parallel;

            if (isParallel) {
                return ParallelHelper.DoOrdered<T, E>(
                    this.parent, 
                    (x, list) => list.Add(new ValueTask<E>(Task.Run(() => this.selector(x).AsTask()))), 
                    cancellationToken);
            }
            else {
                return ParallelHelper.DoOrdered<T, E>(
                    this.parent, 
                    (x, list) => list.Add(this.selector(x)), 
                    cancellationToken);
            }
        }
    }
}