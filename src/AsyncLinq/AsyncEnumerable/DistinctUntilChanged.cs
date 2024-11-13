using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> DistinctUntilChanged<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new DistinctUntilChangedOperator<TSource>(source, pars);
    }

    private class DistinctUntilChangedOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;

        public AsyncOperatorParams Params { get; }

        public DistinctUntilChangedOperator(IAsyncEnumerable<T> parent, AsyncOperatorParams pars) {
            this.parent = parent;
            this.Params = pars;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await using var iterator = this.parent.GetAsyncEnumerator(cancellationToken);
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
    }
}