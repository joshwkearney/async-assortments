using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> AsyncPrepend<T>(this IAsyncEnumerable<T> sequence, Func<ValueTask<T>> newItemTask) {
        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new AsyncPrependOperator<T>(collection, newItemTask);
        }

        return AsyncPrependHelper(sequence, newItemTask);
    }

    private static async IAsyncEnumerable<T> AsyncPrependHelper<T>(this IAsyncEnumerable<T> sequence, Func<ValueTask<T>> newItem) {
        yield return await newItem();

        await foreach (var item in sequence) {
            yield return item;
        }
    }

    private class AsyncPrependOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<ValueTask<T>> newItem;

        public AsyncPrependOperator(IAsyncEnumerableOperator<T> parent, Func<ValueTask<T>> item) {
            this.parent = parent;
            this.newItem = item;
        }

        public int Count => this.parent.Count < 0 ? -1 : this.parent.Count + 1;

        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return AsyncPrependHelper(this.parent, this.newItem).GetAsyncEnumerator(cancellationToken);
        }
    }
}