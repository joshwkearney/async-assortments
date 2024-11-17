﻿using System.Threading.Channels;

namespace AsyncLinq.Operators {
    internal interface ISelectWhereOperator<T> : IAsyncOperator<T> {
        public IAsyncEnumerable<E> SelectWhere<E>(SelectWhereFunc<T, E> nextSelector);
    }

    internal record struct SelectWhereResult<T>(bool IsValid, T Value);

    internal delegate SelectWhereResult<E> SelectWhereFunc<T, E>(T item);

    internal class SelectWhereOperator<T, E> : IAsyncOperator<E>, ISelectWhereOperator<E>, 
        ISelectWhereTaskOperator<E> {

        private readonly IAsyncEnumerable<T> parent;
        private readonly SelectWhereFunc<T, E> selector;

        public AsyncOperatorParams Params { get; }

        public SelectWhereOperator(
            AsyncOperatorParams pars,
            IAsyncEnumerable<T> parent,
            SelectWhereFunc<T, E> selector) {

            this.parent = parent;
            this.selector = selector;
            this.Params = pars;
        }
        
        public IAsyncOperator<E> WithParams(AsyncOperatorParams pars) {
            return new SelectWhereOperator<T, E>(pars, this.parent, this.selector);
        }

        public IAsyncEnumerable<G> SelectWhere<G>(SelectWhereFunc<E, G> nextSelector) {
            return new SelectWhereOperator<T, G>(this.Params, this.parent, newSelector);

            SelectWhereResult<G> newSelector(T item) {
                var (isValid, value) = this.selector(item);

                if (!isValid) {
                    return new(false, default!);
                }

                return nextSelector(value);
            }
        }

        public IAsyncEnumerable<G> SelectWhereTask<G>(AsyncSelectWhereFunc<E, G> nextSelector) {
            return new SelectWhereTaskOperator<T, G>(this.Params, this.parent, newSelector);

            ValueTask<SelectWhereResult<G>> newSelector(T item, CancellationToken token) {
                var (isValid, value) = this.selector(item);

                if (!isValid) {
                    var result = new SelectWhereResult<G>(false, default!);

                    return new ValueTask<SelectWhereResult<G>>(result);
                }

                return nextSelector(value, token);
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
