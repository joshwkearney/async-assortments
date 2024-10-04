using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<E> SelectMany<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
        if (sequence is IAsyncEnumerableOperator<T> op) {
            return new SelectManyAsyncOperator<T, E>(op, selector);
        }

        return SelectManyHelper(sequence, selector);
    }

    public static IAsyncEnumerable<E> SelectMany<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IEnumerable<E>> selector) {
        return sequence.Select(x => selector(x).AsAsyncEnumerable()).SelectMany(x => x);
    }

    private static async IAsyncEnumerable<E> SelectManyHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
        await foreach (var item in sequence) {
            await foreach (var sub in selector(item)) {
                yield return sub;
            }
        }
    }

    private static IAsyncEnumerable<E> ParallelSelectManyHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
        return sequence.DoParallel<T, E>(async (item, channel) => {
            await foreach (var sub in selector(item)) {
                channel.Writer.TryWrite(sub);
            }
        });
    }

    private static IAsyncEnumerable<E> ConcurrentSelectManyHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
        return sequence.DoConcurrent<T, E>(async (item, channel) => {
            await foreach (var sub in selector(item)) {
                channel.Writer.TryWrite(sub);
            }
        });
    }

    private class SelectManyAsyncOperator<T, E> : IAsyncEnumerableOperator<E> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<T, IAsyncEnumerable<E>> selector;

        public int Count => this.parent.Count;

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