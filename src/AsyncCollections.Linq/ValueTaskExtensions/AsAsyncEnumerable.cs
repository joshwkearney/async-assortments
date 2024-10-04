
namespace CollectionTesting {
    public static partial class ValueTaskExtensions {
        public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this ValueTask<T> task) {
            return new ValueTaskToAsyncEnumerable<T>(task);
        }

        private class ValueTaskToAsyncEnumerable<T> : IAsyncEnumerableOperator<T> {
            private readonly ValueTask<T> item;

            public int Count => 1;

            public AsyncExecutionMode ExecutionMode => AsyncExecutionMode.Sequential;

            public ValueTaskToAsyncEnumerable(ValueTask<T> item) {
                this.item = item;
            }

            public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
                yield return await this.item;
            }
        }
    }
}
