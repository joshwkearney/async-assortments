using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCollections.Linq.Operators {
    internal class WrapEnumerableOperator<T> : IAsyncOperator<T>, ISkipTakeOperator<T>, ISelectOperator<T>, 
        IWhereOperator<T>, IConcatEnumerablesOperator<T>, ICountOperator<T>, IToListOperator<T> {

        public IEnumerable<T> Items { get; }

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public WrapEnumerableOperator(AsyncEnumerableScheduleMode pars, IEnumerable<T> items) {
            this.ScheduleMode = pars;
            this.Items = items;
        }
        
        public IAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode pars) {
            return new WrapEnumerableOperator<T>(pars, this.Items);
        }

        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
            var seq = before.Concat(this.Items).Concat(after);

            return new WrapEnumerableOperator<T>(this.ScheduleMode, seq);
        }

        public IAsyncEnumerable<T> SkipTake(int skip, int take) {
            var seq = this.Items.Skip(skip).Take(take);

            return new WrapEnumerableOperator<T>(this.ScheduleMode, seq);
        }

        public int Count() => this.Items.Count();


        public IAsyncEnumerable<E> Select<E>(Func<T, E> selector) {
            var seq = this.Items.Select(selector);

            return new WrapEnumerableOperator<E>(this.ScheduleMode, seq);
        }

        public IAsyncEnumerable<T> Where(Func<T, bool> predicate) {
            var seq = this.Items.Where(predicate);

            return new WrapEnumerableOperator<T>(this.ScheduleMode, seq);
        }

        public ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default) {
            return new ValueTask<List<T>>(this.Items.ToList());
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
