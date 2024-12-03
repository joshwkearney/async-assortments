using System;
using AsyncAssortments.Linq;

namespace AsyncAssortments.Operators {
	internal class DistinctOperator<T> : IAsyncOperator<T>, IToHashSetOperator<T> {
        private readonly IAsyncEnumerable<T> source;
        private readonly IEqualityComparer<T> comparer;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public DistinctOperator(
            AsyncEnumerableScheduleMode mode,
            IAsyncEnumerable<T> source,
            IEqualityComparer<T> comparer) {

            this.source = source;
            this.comparer = comparer;
            this.ScheduleMode = mode;
        }


        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            var set = new HashSet<T>(this.comparer);

            await foreach (var item in this.source.WithCancellation(cancellationToken)) {
                if (set.Add(item)) {
                    yield return item;
                }
            }
        }

        public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars) {
            return new DistinctOperator<T>(pars, this.source, this.comparer);
        }

        public async ValueTask<HashSet<T>> ToHashSetAsync(
            IEqualityComparer<T> comparer,
            CancellationToken cancellationToken = default) {

            var set = await this.source.ToHashSetAsync(this.comparer, cancellationToken);

            if (this.comparer == comparer) {
                return set;
            }
            else {
                return new HashSet<T>(set, comparer);
            }
        }
    }
}