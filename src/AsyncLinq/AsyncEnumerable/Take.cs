using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Take<TSource>(
        this IAsyncEnumerable<TSource> source, 
        int numToTake) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (numToTake < 0) {
            throw new ArgumentOutOfRangeException(nameof(numToTake), "Cannot take less than zero elements");
        }

        if (source is IAsyncEnumerableOperator<TSource> collection) {
            return new TakeOperator<TSource>(collection, numToTake);
        }

        return TakeHelper(source, numToTake);
    }

    private static async IAsyncEnumerable<T> TakeHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        int numToTake,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        int taken = 0;

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
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