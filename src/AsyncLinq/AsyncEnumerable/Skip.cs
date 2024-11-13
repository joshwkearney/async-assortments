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

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new SkipOperator<TSource>(source, numToSkip, pars);
    }

    private class SkipOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly int numToSkip;

        public AsyncOperatorParams Params { get; }

        public SkipOperator(IAsyncEnumerable<T> parent, int numToSkip, AsyncOperatorParams pars) {
            this.parent = parent;
            this.numToSkip = numToSkip;
            this.Params = pars;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await using var iterator = this.parent.GetAsyncEnumerator(cancellationToken);
            int skipped = 0;

            while (skipped < this.numToSkip) {
                var hasNext = await iterator.MoveNextAsync();

                if (!hasNext) {
                    yield break;
                }

                skipped++;
            }

            while (await iterator.MoveNextAsync()) {
                yield return iterator.Current;
            }
        }
    }
}