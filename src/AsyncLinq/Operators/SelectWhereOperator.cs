using System.Threading.Channels;

namespace AsyncLinq.Operators {
    internal interface ISelectWhereOperator<T> : IAsyncOperator<T> {
        public IAsyncEnumerable<E> SelectWhere<E>(SelectWhereFunc<T, E> nextSelector);
    }

    internal record struct SelectWhereResult<T>(bool IsValid, T Value);

    internal delegate SelectWhereResult<E> SelectWhereFunc<T, E>(T item);

    internal class SelectWhereOperator<T, E> : IAsyncOperator<E>, ISelectWhereOperator<E>, IAsyncSelectWhereOperator<E> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly SelectWhereFunc<T, E> selector;

        public AsyncOperatorParams Params { get; }

        public SelectWhereOperator(
            IAsyncEnumerable<T> collection,
            SelectWhereFunc<T, E> selector,
            AsyncOperatorParams pars) {

            this.parent = collection;
            this.selector = selector;
            this.Params = pars;
        }

        public async IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                var (isValid, value) = this.selector(item);

                if (isValid) {
                    yield return value;
                }
            }
        }

        public IAsyncEnumerable<G> SelectWhere<G>(SelectWhereFunc<E, G> nextSelector) {
            return new SelectWhereOperator<T, G>(
                this.parent, 
                x => {
                    var (isValid, value) = this.selector(x);

                    if (!isValid) {
                        return new(false, default!);
                    }

                    return nextSelector(value);
                },
                this.Params);
        }

        public IAsyncEnumerable<G> AsyncSelectWhere<G>(AsyncSelectWhereFunc<E, G> nextSelector) {
            return new AsyncSelectWhereOperator<T, G>(
                this.parent,
                async x => {
                    var (isValid, value) = this.selector(x);

                    if (!isValid) {
                        return new(false, default!);
                    }

                    return await nextSelector(value);
                },
                this.Params);
        }
    }
}
