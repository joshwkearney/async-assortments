using AsyncAssortments.Linq;

namespace AsyncAssortments.Operators {
	internal class ExceptByOperator<TSource, TKey> : IAsyncOperator<TSource> {
        private readonly IAsyncEnumerable<TSource> source;
        private readonly IAsyncEnumerable<TKey> other;
        private readonly Func<TSource, TKey> keySelector;
        private readonly IEqualityComparer<TKey> comparer;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public ExceptByOperator(
            AsyncEnumerableScheduleMode mode,
            IAsyncEnumerable<TSource> source,
            IAsyncEnumerable<TKey> other,
            Func<TSource, TKey> keySelector,
            IEqualityComparer<TKey> comparer) {

            this.source = source;
            this.other = other;
            this.ScheduleMode = mode;
            this.keySelector = keySelector;
            this.comparer = comparer;
        }

        public IAsyncEnumerator<TSource> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ScheduleMode == AsyncEnumerableScheduleMode.Sequential) {
                return this.SequentialIterator(cancellationToken);
            }
            else {
                return this.ConcurrentIterator(cancellationToken);
            }
        }

        private async IAsyncEnumerator<TSource> SequentialIterator(CancellationToken cancellationToken) {
            var second = await this.other.ToHashSetAsync(this.comparer, cancellationToken);

            await foreach (var item in this.source.WithCancellation(cancellationToken)) {
                if (second.Add(this.keySelector(item))) {
                    yield return item;
                }
            }
        }

        private async IAsyncEnumerator<TSource> ConcurrentIterator(CancellationToken cancellationToken) {
            var secondTask = this.other.ToHashSetAsync(this.comparer, cancellationToken);

            // Start fetching the first item while we enumerate the second sequence
            await using var iterator = this.source.GetAsyncEnumerator(cancellationToken);
            
            var nextTask = iterator.MoveNextAsync();
            var second = await secondTask;

            while (await nextTask) {
                var item = iterator.Current;

                if (second.Add(this.keySelector(item))) {
                    yield return item;
                }

                nextTask = iterator.MoveNextAsync();
            }
        }

        public IAsyncOperator<TSource> WithScheduleMode(AsyncEnumerableScheduleMode pars) {
            return new ExceptByOperator<TSource, TKey>(pars, this.source, this.other, this.keySelector, this.comparer);
        }
    }
}
