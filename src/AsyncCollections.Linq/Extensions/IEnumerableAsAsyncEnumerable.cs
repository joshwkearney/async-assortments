namespace AsyncCollections.Linq;

public static partial class AsyncCollectionsExtensions {
    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> sequence) {
        return new ReadOnlyAsyncCollection<T>(sequence);
    }

    private class ReadOnlyAsyncCollection<T> : IAsyncEnumerableOperator<T> {
        private readonly IEnumerable<T> collection;

        public AsyncExecutionMode ExecutionMode => AsyncExecutionMode.Sequential;

        public ReadOnlyAsyncCollection(IEnumerable<T> collection) {
            this.collection = collection;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            foreach (var item in collection) {
                cancellationToken.ThrowIfCancellationRequested();

                yield return item;
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}