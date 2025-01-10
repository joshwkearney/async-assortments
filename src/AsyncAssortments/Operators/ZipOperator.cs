
namespace AsyncAssortments.Operators {
    internal class ZipOperator<T, E> : IAsyncOperator<(T first, E second)> {
        private readonly IAsyncEnumerable<T> parent1;
        private readonly IAsyncEnumerable<E> parent2;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public ZipOperator(
            AsyncEnumerableScheduleMode pars,
            IAsyncEnumerable<T> parent1,
            IAsyncEnumerable<E> parent2) {

            this.parent1 = parent1;
            this.parent2 = parent2;
            this.ScheduleMode = pars;
        }
        
        public IAsyncOperator<(T first, E second)> WithScheduleMode(AsyncEnumerableScheduleMode pars) {
            return new ZipOperator<T, E>(pars, this.parent1, this.parent2);
        }

        public async IAsyncEnumerator<(T, E)> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await using var iterator1 = this.parent1.GetAsyncEnumerator(cancellationToken);
            await using var iterator2 = this.parent2.GetAsyncEnumerator(cancellationToken);

            while (true) {
                var hasFirst = await iterator1.MoveNextAsync();
                var hasSecond = await iterator2.MoveNextAsync();

                if (!hasFirst || !hasSecond) {
                    break;
                }

                yield return (iterator1.Current, iterator2.Current);
            }
        }
    }
}
