using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> AsyncWhere<T>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<bool>> selector) {
        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new AsyncWhereOperator<T>(collection, selector);
        }

        return AsyncWhereHelper(sequence, selector);
    }

    private static async IAsyncEnumerable<T> AsyncWhereHelper<T>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<bool>> selector) {
        await foreach (var item in sequence) {
            if (await selector(item)) {
                yield return item;
            }
        }
    }

    private static IAsyncEnumerable<T> ConcurrentAsyncWhereHelper<T>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<bool>> selector) {
        return sequence.DoConcurrent<T, T>(async (item, channel) => {
            if (await selector(item)) {
                channel.Writer.TryWrite(item);
            }
        });
    }

    private static IAsyncEnumerable<T> ParallelAsyncWhereHelper<T>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<bool>> selector) {
        return sequence.DoParallel<T, T>(async (item, channel) => {
            if (await selector(item)) {
                channel.Writer.TryWrite(item);
            }
        });
    }

    private class AsyncWhereOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<T, ValueTask<bool>> selector;

        public int Count => -1;

        public AsyncExecutionMode ExecutionMode { get; }

        public AsyncWhereOperator(IAsyncEnumerableOperator<T> collection, Func<T, ValueTask<bool>> selector) {
            this.parent = collection;
            this.selector = selector;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Parallel) {
                return ParallelAsyncWhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncExecutionMode.Concurrent) {
                return ConcurrentAsyncWhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return AsyncWhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}