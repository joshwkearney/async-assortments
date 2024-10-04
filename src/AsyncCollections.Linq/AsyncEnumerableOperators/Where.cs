using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> sequence, Func<T, bool> selector) {
        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new WhereOperator<T>(collection, selector);
        }

        return WhereHelper(sequence, selector);
    }

    private static async IAsyncEnumerable<T> WhereHelper<T>(this IAsyncEnumerable<T> sequence, Func<T, bool> selector) {
        await foreach (var item in sequence) {
            if (selector(item)) {
                yield return item;
            }
        }
    }

    private static IAsyncEnumerable<T> ParallelWhereHelper<T>(this IAsyncEnumerable<T> sequence, Func<T, bool> selector) {
        return sequence.DoParallel<T, T>((item, channel) => {
            if (selector(item)) {
                channel.Writer.TryWrite(item);
            }

            return ValueTask.CompletedTask;
        });
    }

    private class WhereOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<T, bool> selector;

        public int Count => -1;

        public AsyncExecutionMode ExecutionMode { get; }

        public WhereOperator(IAsyncEnumerableOperator<T> collection, Func<T, bool> selector) {
            this.parent = collection;
            this.selector = selector;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Parallel) {
                return ParallelWhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return WhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}