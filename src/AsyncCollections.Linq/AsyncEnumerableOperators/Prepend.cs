using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> Prepend<T>(this IAsyncEnumerable<T> sequence, T newItem) {
        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new PrependOperator<T>(collection, newItem);
        }

        return PrependHelper(sequence, newItem);
    }

    private static async IAsyncEnumerable<T> PrependHelper<T>(this IAsyncEnumerable<T> sequence, T newItem) {
        await foreach (var item in sequence) {
            yield return item;
        }

        yield return newItem;
    }

    private class PrependOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly T newItem;

        public PrependOperator(IAsyncEnumerableOperator<T> parent, T item) {
            this.parent = parent;
            this.newItem = item;
        }

        public int Count => this.parent.Count < 0 ? -1 : this.parent.Count + 1;

        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return PrependHelper(this.parent, newItem).GetAsyncEnumerator(cancellationToken);
        }
    }
}