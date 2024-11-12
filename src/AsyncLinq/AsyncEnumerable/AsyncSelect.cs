using System.Runtime.CompilerServices;

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

        if (source is IAsyncLinqOperator<TSource> op) {
            return new AsyncSelectingOperator<TSource, TResult>(op, selector);
        }

        return AsyncSelectHelper(source, selector);
    }

    private static async IAsyncEnumerable<E> AsyncSelectHelper<T, E>(
        this IAsyncEnumerable<T> sequence, 
        Func<T, ValueTask<E>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            yield return await selector(item);
        }
    }

    private static IAsyncEnumerable<E> ParallelAsyncSelectHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<E>> selector) {
        return sequence.DoParallel<T, E>(async (item, channel) => {
            var selected = await selector(item);

            await channel.Writer.WriteAsync(selected);
        });
    }

    private static IAsyncEnumerable<E> ConcurrentAsyncSelectHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<E>> selector) {
        return sequence.DoConcurrent<T, E>(async (item, channel) => {
            var selected = await selector(item);

            await channel.Writer.WriteAsync(selected);
        });
    }

    private class AsyncSelectingOperator<T, E> : IAsyncLinqOperator<E> {
        private readonly IAsyncLinqOperator<T> parent;
        private readonly Func<T, ValueTask<E>> selector;
        
        public AsyncLinqExecutionMode ExecutionMode { get; }

        public AsyncSelectingOperator(IAsyncLinqOperator<T> collection, Func<T, ValueTask<E>> selector) {
            this.parent = collection;
            this.selector = selector;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncLinqExecutionMode.Parallel) {
                return ParallelAsyncSelectHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncLinqExecutionMode.Concurrent) {
                return ConcurrentAsyncSelectHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return AsyncSelectHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}