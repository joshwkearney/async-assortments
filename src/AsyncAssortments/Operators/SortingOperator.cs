using AsyncAssortments.Linq;

namespace AsyncAssortments.Operators {
    internal class SortingOperator<T> : IAsyncOperator<T>, IOrderedAsyncEnumerable<T> {
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

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            var list = await this.Source.ToListAsync(cancellationToken);

            list.Sort(this.Comparer);

            foreach (var item in list) {
                yield return item;
            }
        }

        public IAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode mode) {
            return new SortingOperator<T>(mode, this.Source, this.Comparer);
        }
    }
}
