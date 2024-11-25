namespace AsyncCollections.Linq.Operators {
    internal class SkipTakeOperator<T> : IAsyncOperator<T>, ISkipTakeOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly int skip;
        private readonly int take;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public SkipTakeOperator(AsyncEnumerableScheduleMode pars, IAsyncEnumerable<T> parent, int skip, int take) {
            this.parent = parent;
            this.skip = skip;
            this.take = take;
            this.ScheduleMode = pars;
        }
        
        public IAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode pars) {
            return new SkipTakeOperator<T>(pars, parent, skip, take);
        }

        public IAsyncEnumerable<T> SkipTake(int skip, int take) {
            return new SkipTakeOperator<T>(
                this.ScheduleMode,
                this.parent, 
                this.skip + skip, 
                Math.Min(this.take - skip, take));
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await using var iterator = this.parent.GetAsyncEnumerator(cancellationToken);

            int skipped = 0;
            int taken = 0;

            while (skipped < this.skip) {
                var hasNext = await iterator.MoveNextAsync();

                if (!hasNext) {
                    yield break;
                }

                skipped++;
            }

            while (taken < this.take) {
                var hasNext = await iterator.MoveNextAsync();

                if (!hasNext) {
                    yield break;
                }

                yield return iterator.Current;
                taken++;
            }
        }
    }
}
