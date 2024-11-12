using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<T> DistinctUntilChanged<T>(this IAsyncEnumerable<T> sequence) {
        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        if (sequence is IAsyncEnumerableOperator<T> op) {
            return new DistinctUntilChangedOperator<T>(op);
        }

        return DistinctUntilChangedHelper(sequence);
    }

    private static async IAsyncEnumerable<T> DistinctUntilChangedHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await using var iterator = sequence.GetAsyncEnumerator(cancellationToken);
        var hasFirst = await iterator.MoveNextAsync();

        if (!hasFirst) {
            yield break;
        }

        var current = iterator.Current;
        yield return current;

        while (await iterator.MoveNextAsync()) {
            var item = iterator.Current;

            if (EqualityComparer<T>.Default.Equals(item, current)) {
                continue;
            }

            current = item;
            yield return current;
        }
    }

    private class DistinctUntilChangedOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;

        public int Count => -1;

        public AsyncExecutionMode ExecutionMode { get; }

        public DistinctUntilChangedOperator(IAsyncEnumerableOperator<T> parent) {
            this.parent = parent;
            this.ExecutionMode = parent.ExecutionMode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return DistinctUntilChangedHelper(this.parent).GetAsyncEnumerator(cancellationToken);
        }
    }
}