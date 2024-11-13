using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Where<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, bool> predicate) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new WhereOperator<TSource>(source, predicate, pars);
    }

    private class WhereOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly Func<T, bool> selector;

        public AsyncOperatorParams Params { get; }

        public WhereOperator(IAsyncEnumerable<T> collection, Func<T, bool> selector, AsyncOperatorParams pars) {
            this.parent = collection;
            this.selector = selector;
            this.Params = pars;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                if (this.selector(item)) {
                    yield return item;
                }
            }
        }
    }
}