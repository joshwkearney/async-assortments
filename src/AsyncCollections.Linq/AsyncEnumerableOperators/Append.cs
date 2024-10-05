using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> Append<T>(this IAsyncEnumerable<T> sequence, T newItem) {
        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new AppendOperator<T>(collection, newItem);
        }

        return AppendHelper(sequence, newItem);
    }

    private static async IAsyncEnumerable<T> AppendHelper<T>(this IAsyncEnumerable<T> sequence, T newItem) {
        await foreach (var item in sequence) {
            yield return item;
        }

        yield return newItem;
    }

    private class AppendOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly T toAppend;

        public AppendOperator(IAsyncEnumerableOperator<T> parent, T item) {
            this.parent = parent;
            this.toAppend = item;
        }
        
        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return AppendHelper(this.parent, this.toAppend).GetAsyncEnumerator(cancellationToken);
        }
    }
}