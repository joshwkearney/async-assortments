using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> Skip<T>(this IAsyncEnumerable<T> sequence, int numToSkip) {
        if (numToSkip < 0) {
            throw new ArgumentOutOfRangeException(nameof(numToSkip), "Cannot skip less than zero elements");
        }

        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new SkipOperator<T>(collection, numToSkip);
        }

        return SkipHelper(sequence, numToSkip);
    }

    private static async IAsyncEnumerable<T> SkipHelper<T>(this IAsyncEnumerable<T> sequence, int numToSkip) {
        int skipped = 0;

        await foreach (var item in sequence) {
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

        public int Count => this.parent.Count < 0 ? -1 : Math.Max(0, this.parent.Count - this.numToSkip);

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