using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncLinq.Operators {
    internal class EnumerableOperator<T> : IAsyncOperator<T>, ISkipTakeOperator<T>, ISelectWhereOperator<T>, 
        IConcatEnumerablesOperator<T> {

        public IEnumerable<T> Items { get; }

        public AsyncOperatorParams Params { get; }

        public EnumerableOperator(AsyncOperatorParams pars, IEnumerable<T> items) {
            this.Params = pars;
            this.Items = items;
        }
        
        public IAsyncOperator<T> WithParams(AsyncOperatorParams pars) {
            return new EnumerableOperator<T>(pars, this.Items);
        }

        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
            var seq = before.Concat(this.Items).Concat(after);

            return new EnumerableOperator<T>(this.Params, seq);
        }

        public IAsyncEnumerable<T> SkipTake(int skip, int take) {
            var seq = this.Items.Skip(skip).Take(take);

            return new EnumerableOperator<T>(this.Params, seq);
        }

        public IAsyncEnumerable<E> SelectWhere<E>(SelectWhereFunc<T, E> nextSelector) {
            var seq = SelectWhereHelper(this.Items, nextSelector);

            return new EnumerableOperator<E>(this.Params, seq);
        }
        
        private static IEnumerable<E> SelectWhereHelper<E>(IEnumerable<T> seq, SelectWhereFunc<T, E> selector) {
            foreach (var item in seq) {
                var (isValid, value) = selector(item);

                if (isValid) {
                    yield return value;
                }
            }
        }
        
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            foreach (var item in Items) {
                yield return item;
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}
