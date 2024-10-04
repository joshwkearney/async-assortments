using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> AsyncAppend<T>(this IAsyncEnumerable<T> sequence, Func<ValueTask<T>> newItemTask) {
        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new AsyncAppendOperator<T>(collection, newItemTask);
        }

        return AsyncAppendHelper(sequence, newItemTask);
    }

    private static async IAsyncEnumerable<T> AsyncAppendHelper<T>(this IAsyncEnumerable<T> sequence, Func<ValueTask<T>> newItem) {
        await foreach (var item in sequence) {
            yield return item;
        }

        yield return await newItem();
    }

    private static async IAsyncEnumerable<T> ConcurrentAsyncAppendHelper<T>(this IAsyncEnumerable<T> sequence, Func<ValueTask<T>> newItem) {
        var newItemTask = newItem();
        
        await foreach (var item in sequence) {
            yield return item;
        }

        yield return await newItemTask;
    }

    private static async IAsyncEnumerable<T> ParallelAsyncAppendHelper<T>(this IAsyncEnumerable<T> sequence, Func<ValueTask<T>> newItem) {
        var newItemTask = Task.Run(async () => await newItem());

        await foreach (var item in sequence) {
            yield return item;
        }

        yield return await newItemTask;
    }

    private class AsyncAppendOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<ValueTask<T>> toAppend;

        public AsyncAppendOperator(IAsyncEnumerableOperator<T> parent, Func<ValueTask<T>> item) {
            this.parent = parent;
            this.toAppend = item;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public int Count => this.parent.Count < 0 ? -1 : this.parent.Count + 1;

        public AsyncExecutionMode ExecutionMode { get; }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Concurrent) {
                return ConcurrentAsyncAppendHelper(this.parent, this.toAppend).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncExecutionMode.Parallel) {
                return ParallelAsyncAppendHelper(this.parent, this.toAppend).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return AsyncAppendHelper(this.parent, this.toAppend).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}