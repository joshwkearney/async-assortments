using System.Threading.Channels;

namespace AsyncLinq.Operators {
    internal record struct SelectWhereResult<T>(bool IsValid, T Value);

    internal delegate SelectWhereResult<E> SelectWhereFunc<T, E>(T item);

    internal class SelectWhereOperator<T, E> : IAsyncOperator<E>, ISelectOperator<E>, 
        IWhereOperator<E>, IAsyncSelectOperator<E>, IAsyncWhereOperator<E> {

        private readonly IAsyncEnumerable<T> parent;
        private readonly SelectWhereFunc<T, E> selector;

        public AsyncPipelineExecution Execution { get; }

        public SelectWhereOperator(
            AsyncPipelineExecution pars,
            IAsyncEnumerable<T> parent,
            SelectWhereFunc<T, E> selector) {

            this.parent = parent;
            this.selector = selector;
            this.Execution = pars;
        }
        
        public IAsyncOperator<E> WithExecution(AsyncPipelineExecution pars) {
            return new SelectWhereOperator<T, E>(pars, this.parent, this.selector);
        }

        public IAsyncEnumerable<G> Select<G>(Func<E, G> selector) {
            return new SelectWhereOperator<T, G>(this.Execution, this.parent, newSelector);

            SelectWhereResult<G> newSelector(T item) {
                var (isValid, value) = this.selector(item);

                if (!isValid) {
                    return new(false, default!);
                }

                return new(true, selector(value));
            }
        }

        public IAsyncEnumerable<E> Where(Func<E, bool> predicate) {
            return new SelectWhereOperator<T, E>(this.Execution, this.parent, newSelector);

            SelectWhereResult<E> newSelector(T item) {
                var (isValid, value) = this.selector(item);

                if (!isValid) {
                    return new(false, default!);
                }

                if (!predicate(value)) {
                    return new(false, default!);
                }

                return new(true, value);
            }
        }

        public IAsyncEnumerable<G> AsyncSelect<G>(Func<E, CancellationToken, ValueTask<G>> nextSelector) {
            return new SelectWhereTaskOperator<T, G>(this.Execution, this.parent, newSelector);

            async ValueTask<SelectWhereResult<G>> newSelector(T item, CancellationToken token) {
                var (isValid, value) = this.selector(item);

                if (!isValid) {
                    return new SelectWhereResult<G>(false, default!);
                }

                return new SelectWhereResult<G>(true, await nextSelector(value, token));
            }
        }

        public IAsyncEnumerable<E> AsyncWhere(Func<E, CancellationToken, ValueTask<bool>> predicate) {
            return new SelectWhereTaskOperator<T, E>(this.Execution, this.parent, newSelector);

            async ValueTask<SelectWhereResult<E>> newSelector(T item, CancellationToken token) {
                var (isValid, value) = this.selector(item);

                if (!isValid) {
                    return new SelectWhereResult<E>(false, default!);
                }

                return new SelectWhereResult<E>(await predicate(value, token), value);
            }
        }

        public async IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                var (isValid, value) = this.selector(item);

                if (isValid) {
                    yield return value;
                }
            }
        }
    }
}
