using System;
using AsyncAssortments.Linq;

namespace AsyncAssortments.Operators {
    internal class ReverseOperator<T> : IAsyncOperator<T>, IToListOperator<T>, IToHashSetOperator<T>,
        IReverseOperator<T> {

        private readonly IAsyncEnumerable<T> source;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public int MaxConcurrency { get; }

        public ReverseOperator(
            AsyncEnumerableScheduleMode mode,
            int maxConcurrency,
            IAsyncEnumerable<T> source) {

            this.ScheduleMode = mode;
            this.MaxConcurrency = maxConcurrency;
            this.source = source;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            var list = await this.source.ToListAsync(cancellationToken);

            for (int i = list.Count - 1; i >= 0; i--) {
                yield return list[i];
            }
        }

        public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars, int maxConcurrency) {
            return new ReverseOperator<T>(pars, maxConcurrency, this.source);
        }

        public async ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default) {
            var list = await this.source.ToListAsync(cancellationToken);

            list.Reverse();
            return list;
        }

        public ValueTask<HashSet<T>> ToHashSetAsync(
            IEqualityComparer<T> comparer,
            CancellationToken cancellationToken = default) {

            // Hash sets don't keep track of order, so we don't have to do anything
            return this.source.ToHashSetAsync(comparer, cancellationToken);
        }

        public IAsyncEnumerable<T> Reverse() => this.source;
    }
}

