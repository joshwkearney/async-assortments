using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Skip<TSource>(
        this IAsyncEnumerable<TSource> source, 
        int numToSkip) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (numToSkip < 0) {
            throw new ArgumentOutOfRangeException(nameof(numToSkip), "Cannot skip less than zero elements");
        }

        if (source is IAsyncLinqOperator<TSource> collection) {
            return new SkipOperator<TSource>(collection, numToSkip);
        }

        return SkipHelper(source, numToSkip);
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

    private class SkipOperator<T> : IAsyncLinqOperator<T> {
        private readonly IAsyncLinqOperator<T> parent;
        private readonly int numToSkip;
        
        public AsyncLinqExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public SkipOperator(IAsyncLinqOperator<T> parent, int numToSkip) {
            this.parent = parent;
            this.numToSkip = numToSkip;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return SkipHelper(this.parent, this.numToSkip).GetAsyncEnumerator(cancellationToken);
        }
    }
}