using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncLinq.Operators {
    internal class EnumerableOperator<T> : IAsyncOperator<T>, ISkipTakeOperator<T>, ISelectWhereOperator<T>, IEnumerableConcatOperator<T> {
        private readonly IEnumerable<T> collection;

        public AsyncOperatorParams Params => default;

        public EnumerableOperator(IEnumerable<T> collection) {
            this.collection = collection;
        }

        public IAsyncEnumerable<T> ComposeWith(IEnumerable<T> before, IEnumerable<T> after) {
            var seq = before.Concat(this.collection).Concat(after);

            return new EnumerableOperator<T>(seq);
        }

        public IAsyncEnumerable<T> ComposeWith(int skip, int take) {
            var seq = this.collection.Skip(skip).Take(take);

            return new EnumerableOperator<T>(seq);
        }

        public IAsyncEnumerable<E> ComposeWith<E>(SelectWhereFunc<T, E> nextSelector) {
            var seq = SelectWhereHelper(this.collection, nextSelector);

            return new EnumerableOperator<E>(seq);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            foreach (var item in collection) {
                cancellationToken.ThrowIfCancellationRequested();

                yield return item;
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        private static IEnumerable<E> SelectWhereHelper<E>(IEnumerable<T> seq, SelectWhereFunc<T, E> selector) {
            foreach (var item in seq) {
                var (isValid, value) = selector(item);

                if (isValid) {
                    yield return value;
                }
            }
        }
    }
}
