using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<T> Append<T>(this IAsyncEnumerable<T> sequence, T newItem) {
        if (sequence is IAsyncEnumerableOperator<T> op) {
            if (op.ExecutionMode == AsyncExecutionMode.Sequential) {
                return new AppendOperator<T>(op, newItem);
            }
            else {
                return sequence.Concat(new[] { newItem }.AsAsyncEnumerable());
            }
        }

        return AppendHelper(sequence, newItem);
    }

    private static async IAsyncEnumerable<T> AppendHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        T newItem,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            yield return item;
        }

        cancellationToken.ThrowIfCancellationRequested();
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