using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<T> AsyncPrepend<T>(this IAsyncEnumerable<T> sequence, Func<ValueTask<T>> newItemTask) {
        if (sequence is IAsyncEnumerableOperator<T> op) {
            if (op.ExecutionMode == AsyncExecutionMode.Sequential) {
                return new AsyncPrependOperator<T>(op, newItemTask);
            }
            else if (op.ExecutionMode == AsyncExecutionMode.Parallel) {
                var task = Task.Run(() => newItemTask().AsTask());

                return task.AsAsyncEnumerable().Concat(sequence);
            }
            else {
                return newItemTask().AsAsyncEnumerable().Concat(sequence);
            }
        }

        return AsyncPrependHelper(sequence, newItemTask);
    }

    private static async IAsyncEnumerable<T> AsyncPrependHelper<T>(
        this IAsyncEnumerable<T> sequence,
        Func<ValueTask<T>> newItem,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        // Create the iterator first so subscribe works correctly
        var iterator = sequence.WithCancellation(cancellationToken).GetAsyncEnumerator();
        var moveNextTask = iterator.MoveNextAsync();

        // Then yield the first item
        yield return await newItem();

        // Then yield the rest
        while (await moveNextTask) {
            yield return iterator.Current;
            moveNextTask = iterator.MoveNextAsync();
        }
    }

    private class AsyncPrependOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<ValueTask<T>> newItem;

        public AsyncPrependOperator(IAsyncEnumerableOperator<T> parent, Func<ValueTask<T>> item) {
            this.parent = parent;
            this.newItem = item;
        }
        
        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return AsyncPrependHelper(this.parent, this.newItem).GetAsyncEnumerator(cancellationToken);
        }
    }
}