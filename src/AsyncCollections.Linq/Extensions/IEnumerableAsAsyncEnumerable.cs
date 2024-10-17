namespace AsyncCollections.Linq;

public static partial class AsyncCollectionsExtensions {
    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> sequence) {
        return new ReadOnlyAsyncCollection<T>(sequence, AsyncExecutionMode.Sequential);
    }

    private class ReadOnlyAsyncCollection<T> : IAsyncEnumerableOperator<T> {
        private readonly IEnumerable<T> collection;

        public int Count {
            get {
                if (this.collection is IReadOnlyCollection<T> collection) {
                    return collection.Count;
                }
                else if (this.collection is ICollection<T> collection2) {
                    return collection2.Count;
                }
                else {
                    return -1;
                }
            }
        }

        public AsyncExecutionMode ExecutionMode { get; }

        public ReadOnlyAsyncCollection(IEnumerable<T> collection, AsyncExecutionMode mode) {
            this.collection = collection;
            ExecutionMode = mode;
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