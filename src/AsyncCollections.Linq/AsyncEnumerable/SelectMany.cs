using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<E> SelectMany<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
        if (sequence is IAsyncEnumerableOperator<T> op) {
            return new SelectManyAsyncOperator<T, E>(op, selector);
        }

        return SelectManyHelper(sequence, selector);
    }

    public static IAsyncEnumerable<E> SelectMany<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IEnumerable<E>> selector) {
        return sequence.SelectMany(x => selector(x).AsAsyncEnumerable());
    }

    private static async IAsyncEnumerable<E> SelectManyHelper<T, E>(
        this IAsyncEnumerable<T> sequence, 
        Func<T, IAsyncEnumerable<E>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            await foreach (var sub in selector(item).WithCancellation(cancellationToken)) {
                yield return sub;
            }
        }
    }

    private static IAsyncEnumerable<E> ParallelSelectManyHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
        return sequence.DoParallel<T, E>(async (item, channel) => {
            await foreach (var sub in selector(item)) {
                await channel.Writer.WriteAsync(sub);
            }
        });
    }

    private static IAsyncEnumerable<E> ConcurrentSelectManyHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
        return sequence.DoConcurrent<T, E>(async (item, channel) => {
            await foreach (var sub in selector(item)) {
                await channel.Writer.WriteAsync(sub);
            }
        });
    }

    private class SelectManyAsyncOperator<T, E> : IAsyncEnumerableOperator<E> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<T, IAsyncEnumerable<E>> selector;
        
        public AsyncExecutionMode ExecutionMode { get; }

        public SelectManyAsyncOperator(IAsyncEnumerableOperator<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
            this.parent = sequence;
            this.selector = selector;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Parallel) {
                return ParallelSelectManyHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncExecutionMode.Concurrent) {
                return ConcurrentSelectManyHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return SelectManyHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}