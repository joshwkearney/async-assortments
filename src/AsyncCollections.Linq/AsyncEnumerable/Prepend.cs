using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<T> Prepend<T>(this IAsyncEnumerable<T> sequence, T newItem) {
        if (sequence is IAsyncEnumerableOperator<T> op) {
            if (op.ExecutionMode == AsyncExecutionMode.Sequential) {
                return new PrependOperator<T>(op, newItem);
            }
            else {
                return sequence.Concat(new[] { newItem }.AsAsyncEnumerable());
            }
        }

        return PrependHelper(sequence, newItem);
    }

    private static async IAsyncEnumerable<T> PrependHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        T newItem, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        // Create the iterator first so subscribe works correctly
        var iterator = sequence.WithCancellation(cancellationToken).GetAsyncEnumerator();
        var moveNextTask = iterator.MoveNextAsync();

        // Then yield the prepended item
        cancellationToken.ThrowIfCancellationRequested();
        yield return newItem;

        // Then yield the rest
        while (await moveNextTask) {
            yield return iterator.Current;
            moveNextTask = iterator.MoveNextAsync();
        }
    }

    private class PrependOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly T newItem;

        public PrependOperator(IAsyncEnumerableOperator<T> parent, T item) {
            this.parent = parent;
            this.newItem = item;
        }
        
        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return PrependHelper(this.parent, newItem).GetAsyncEnumerator(cancellationToken);
        }
    }
}