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

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new TakeOperator<TSource>(source, numToTake, pars);
    }

    private class TakeOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly int numToTake;

        public AsyncOperatorParams Params { get; }

        public TakeOperator(IAsyncEnumerable<T> parent, int numToTake, AsyncOperatorParams pars) {
            this.parent = parent;
            this.numToTake = numToTake;
            this.Params = pars;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await using var iterator = this.parent.GetAsyncEnumerator(cancellationToken);
            int taken = 0;

            while (taken < this.numToTake) {
                var hasNext = await iterator.MoveNextAsync();

                if (!hasNext) {
                    break;
                }

                yield return iterator.Current;
                taken++;
            }
        }
    }
}