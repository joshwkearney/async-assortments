/*using AsyncLinq.Operators;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Distinct<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new DistinctOperator<TSource>(source, pars);
    }

    private class DistinctOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;

        public AsyncOperatorParams Params { get; }

        public DistinctOperator(IAsyncEnumerable<T> parent, AsyncOperatorParams pars) {
            this.parent = parent;
            this.Params = pars;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            var set = new HashSet<T>();

            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                if (set.Add(item)) {
                    yield return item;
                }
            }
        }
    }
}*/