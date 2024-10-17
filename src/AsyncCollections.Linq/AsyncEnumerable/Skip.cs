using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<T> Skip<T>(this IAsyncEnumerable<T> sequence, int numToSkip) {
        if (numToSkip < 0) {
            throw new ArgumentOutOfRangeException(nameof(numToSkip), "Cannot skip less than zero elements");
        }

        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new SkipOperator<T>(collection, numToSkip);
        }

        return SkipHelper(sequence, numToSkip);
    }

    private static async IAsyncEnumerable<T> SkipHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        int numToSkip,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        int skipped = 0;

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            if (skipped < numToSkip) {
                skipped++;
                continue;
            }

            yield return item;
        }
    }

    private class SkipOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly int numToSkip;
        
        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public SkipOperator(IAsyncEnumerableOperator<T> parent, int numToSkip) {
            this.parent = parent;
            this.numToSkip = numToSkip;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return SkipHelper(this.parent, this.numToSkip).GetAsyncEnumerator(cancellationToken);
        }
    }
}