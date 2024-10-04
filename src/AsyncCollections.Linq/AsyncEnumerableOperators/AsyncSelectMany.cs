using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<E> AsyncSelectMany<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<IAsyncEnumerable<E>>> selector) {
        if (sequence is IAsyncEnumerableOperator<T> op) {
            return new AsyncSelectManyOperator<T, E>(op, selector);
        }

        return AsyncSelectManyHelper(sequence, selector);
    }

    public static IAsyncEnumerable<E> AsyncSelectMany<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<IEnumerable<E>>> selector) {
        return sequence.AsyncSelect(selector).SelectMany(x => x);
    }

    private static async IAsyncEnumerable<E> AsyncSelectManyHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<IAsyncEnumerable<E>>> selector) {
        await foreach (var item in sequence) {
            await foreach (var sub in await selector(item)) {
                yield return sub;
            }
        }
    }

    private static IAsyncEnumerable<E> ParallelAsyncSelectManyHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<IAsyncEnumerable<E>>> selector) {
        return sequence.DoParallel<T, E>(async (item, channel) => {
            await foreach (var sub in await selector(item)) {
                channel.Writer.TryWrite(sub);
            }
        });
    }

    private static IAsyncEnumerable<E> ConcurrentAsyncSelectManyHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<IAsyncEnumerable<E>>> selector) {
        return sequence.DoConcurrent<T, E>(async (item, channel) => {
            await foreach (var sub in await selector(item)) {
                channel.Writer.TryWrite(sub);
            }
        });
    }

    private class AsyncSelectManyOperator<T, E> : IAsyncEnumerableOperator<E> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<T, ValueTask<IAsyncEnumerable<E>>> selector;

        public int Count => this.parent.Count;

        public AsyncExecutionMode ExecutionMode { get; }

        public AsyncSelectManyOperator(IAsyncEnumerableOperator<T> collection, Func<T, ValueTask<IAsyncEnumerable<E>>> selector) {
            this.parent = collection;
            this.selector = selector;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Parallel) {
                return ParallelAsyncSelectManyHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncExecutionMode.Concurrent) {
                return ConcurrentAsyncSelectManyHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return AsyncSelectManyHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}