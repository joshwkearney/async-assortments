using AsyncAssortments.Linq;

namespace AsyncAssortments.Operators {
    internal class SortingOperator<T> : IAsyncOperator<T>, IOrderedAsyncEnumerable<T>, IToListOperator<T>, 
        IToHashSetOperator<T>, IToSortedSetOperator<T>, IOrderByOperator<T> {

        public IAsyncEnumerable<T> Source { get; }

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public IComparer<T> Comparer { get; }

        public SortingOperator(
            AsyncEnumerableScheduleMode mode, 
            IAsyncEnumerable<T> parent, 
            IComparer<T> comparer) {

            this.Source = parent;
            this.ScheduleMode = mode;
            this.Comparer = comparer;
        }

        public async ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default) {
            var list = await this.Source.ToListAsync(cancellationToken);

            list.Sort(this.Comparer);
            return list;
        }

        public ValueTask<HashSet<T>> ToHashSetAsync(CancellationToken cancellationToken = default) {
            // Creating a hash set destroys ordering anyway, so we don't need to sort anything
            return this.Source.ToHashSetAsync(cancellationToken);
        }

        public ValueTask<SortedSet<T>> ToSortedSetAsync(CancellationToken cancellationToken = default) {
            return this.Source.ToSortedSetAsync(this.Comparer, cancellationToken);
        }

        public IAsyncEnumerable<T> OrderBy(IComparer<T> comparer) {
            return new SortingOperator<T>(this.ScheduleMode, this.Source, comparer);
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            var list = await this.Source.ToListAsync(cancellationToken);

            // TODO: Is there a way to do this incrementally so we don't end up sorting everything
            // at the end?
            list.Sort(this.Comparer);

            foreach (var item in list) {
                yield return item;
            }
        }

        public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode mode) {
            return new SortingOperator<T>(mode, this.Source, this.Comparer);
        }
    }
}
