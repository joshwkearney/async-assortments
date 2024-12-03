using System;
using System.Diagnostics;
using AsyncAssortments.Linq;

namespace AsyncAssortments.Operators {
	internal class ExceptOperator<T> : IAsyncOperator<T>, IToHashSetOperator<T>, IToListOperator<T> {
        private readonly IAsyncEnumerable<T> source;
        private readonly IAsyncEnumerable<T> other;
        private readonly IEqualityComparer<T> comparer;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public ExceptOperator(
            AsyncEnumerableScheduleMode mode,
            IAsyncEnumerable<T> source,
            IAsyncEnumerable<T> other,
            IEqualityComparer<T> comparer) {

            this.source = source;
            this.other = other;
            this.ScheduleMode = mode;
            this.comparer = comparer;

            Debug.Assert(mode.IsUnordered());
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ScheduleMode == AsyncEnumerableScheduleMode.Sequential) {
                return this.SequentialIterator(cancellationToken);
            }
            else {
                return this.ConcurrentIterator(cancellationToken);
            }
        }

        private async IAsyncEnumerator<T> SequentialIterator(CancellationToken cancellationToken) {
            var second = await this.other.ToHashSetAsync(this.comparer, cancellationToken);

            await foreach (var item in this.source.WithCancellation(cancellationToken)) {
                if (!second.Contains(item)) {
                    yield return item;
                }
            }
        }

        private async IAsyncEnumerator<T> ConcurrentIterator(CancellationToken cancellationToken) {
            var secondTask = this.other.ToHashSetAsync(this.comparer, cancellationToken);

            // Start fetching the first item while we enumerate the second sequence
            await using var iterator = this.source.GetAsyncEnumerator(cancellationToken);
            var nextTask = iterator.MoveNextAsync();
            var second = await secondTask;

            while (await nextTask) {
                var item = iterator.Current;

                if (!second.Contains(item)) {
                    yield return item;
                }

                nextTask = iterator.MoveNextAsync();
            }
        }

        public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars) {
            return new ExceptOperator<T>(pars, this.source, this.other, comparer);
        }

        public async ValueTask<HashSet<T>> ToHashSetAsync(
            IEqualityComparer<T> comparer,
            CancellationToken cancellationToken = default) {

            var task1 = this.source.ToHashSetAsync(this.comparer, cancellationToken);
            var task2 = this.other.ToHashSetAsync(this.comparer, cancellationToken);

            var set1 = await task1;
            var set2 = await task2;

            set1.ExceptWith(set2);

            if (this.comparer == comparer) {
                return set1;
            }
            else {
                return new HashSet<T>(set1, comparer);
            }
        }

        public async ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default) {
            var set = await this.ToHashSetAsync(this.comparer, cancellationToken);

            return set.ToList();
        }
    }
}

