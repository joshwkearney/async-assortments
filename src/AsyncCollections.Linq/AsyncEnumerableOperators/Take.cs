using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> Take<T>(this IAsyncEnumerable<T> sequence, int numToTake) {
        if (numToTake < 0) {
            throw new ArgumentOutOfRangeException(nameof(numToTake), "Cannot take less than zero elements");
        }

        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new TakeOperator<T>(collection, numToTake);
        }

        return TakeHelper(sequence, numToTake);
    }

    private static async IAsyncEnumerable<T> TakeHelper<T>(this IAsyncEnumerable<T> sequence, int numToTake) {
        int taken = 0;

        await foreach (var item in sequence) {
            if (taken >= numToTake) {
                yield break;
            }

            yield return item;
            taken++;
        }
    }

    private class TakeOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly int numToTake;

        public int Count => this.parent.Count < 0 ? -1 : Math.Min(this.parent.Count, this.numToTake);

        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public TakeOperator(IAsyncEnumerableOperator<T> parent, int numToTake) {
            this.parent = parent;
            this.numToTake = numToTake;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return TakeHelper(this.parent, this.numToTake).GetAsyncEnumerator(cancellationToken);
        }
    }
}