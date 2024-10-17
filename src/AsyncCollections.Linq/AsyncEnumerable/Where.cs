using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<T> Where<T>(this IAsyncEnumerable<T> sequence, Func<T, bool> selector) {
        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new WhereOperator<T>(collection, selector);
        }

        return WhereHelper(sequence, selector);
    }

    private static async IAsyncEnumerable<T> WhereHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        Func<T, bool> selector, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            if (selector(item)) {
                yield return item;
            }
        }
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
            return WhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
        }
    }
}