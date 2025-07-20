using AsyncAssortments.Linq;

namespace AsyncAssortments.Operators {
    internal class SortingOperator<T> : IAsyncOperator<T>, IOrderedAsyncEnumerable<T>, IToListOperator<T>, 
        IToHashSetOperator<T>, IOrderOperator<T> {

        public IAsyncEnumerable<T> Source { get; }

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public IComparer<T> Comparer { get; }

        public int MaxConcurrency { get; }

        public SortingOperator(
            AsyncEnumerableScheduleMode mode, 
            int maxConcurrency,
            IAsyncEnumerable<T> parent, 
            IComparer<T> comparer) {

            this.Source = parent;
            this.ScheduleMode = mode;
            this.Comparer = comparer;
            this.MaxConcurrency = maxConcurrency;
        }

        public async ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default) {
            var list = await this.Source.ToListAsync(cancellationToken);

            list.Sort(this.Comparer);
            return list;
        }

        public ValueTask<HashSet<T>> ToHashSetAsync(IEqualityComparer<T> comparer, CancellationToken cancellationToken = default) {
            // Creating a hash set destroys ordering anyway, so we don't need to sort anything
            return this.Source.ToHashSetAsync(comparer, cancellationToken);
        }

        public IOrderedAsyncEnumerable<T> Order(IComparer<T> comparer) {
            return new SortingOperator<T>(this.ScheduleMode, this.MaxConcurrency, this.Source, comparer);
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

        public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode mode, int maxConcurrency) {
            return new SortingOperator<T>(mode, maxConcurrency, this.Source, this.Comparer);
        }
    }
}
