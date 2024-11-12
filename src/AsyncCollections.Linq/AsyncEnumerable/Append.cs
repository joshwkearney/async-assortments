using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Append<TSource>(
        this IAsyncEnumerable<TSource> source, 
        TSource newItem) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncEnumerableOperator<TSource> op) {
            if (op.ExecutionMode == AsyncExecutionMode.Sequential) {
                return new AppendOperator<TSource>(op, newItem);
            }
            else {
                return source.Concat(new[] { newItem }.AsAsyncEnumerable());
            }
        }

        return AppendHelper(source, newItem);
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