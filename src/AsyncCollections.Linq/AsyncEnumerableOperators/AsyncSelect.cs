using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<E> AsyncSelect<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<E>> selector) {
        if (sequence is IAsyncEnumerableOperator<T> op) {
            return new AsyncSelectingOperator<T, E>(op, selector);
        }

        return AsyncSelectHelper(sequence, selector);
    }

    private static async IAsyncEnumerable<E> AsyncSelectHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<E>> selector) {
        await foreach (var item in sequence) {
            yield return await selector(item);
        }
    }

    private static IAsyncEnumerable<E> ParallelAsyncSelectHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<E>> selector) {
        return sequence.DoParallel<T, E>(async (item, channel) => {
            var selected = await selector(item);

            channel.Writer.TryWrite(selected);
        });
    }

    private static IAsyncEnumerable<E> ConcurrentAsyncSelectHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<E>> selector) {
        return sequence.DoConcurrent<T, E>(async (item, channel) => {
            var selected = await selector(item);

            channel.Writer.TryWrite(selected);
        });
    }

    private class AsyncSelectingOperator<T, E> : IAsyncEnumerableOperator<E> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<T, ValueTask<E>> selector;

        public int Count => this.parent.Count;

        public AsyncExecutionMode ExecutionMode { get; }

        public AsyncSelectingOperator(IAsyncEnumerableOperator<T> collection, Func<T, ValueTask<E>> selector) {
            this.parent = collection;
            this.selector = selector;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Parallel) {
                return ParallelAsyncSelectHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncExecutionMode.Concurrent) {
                return ConcurrentAsyncSelectHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return AsyncSelectHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}