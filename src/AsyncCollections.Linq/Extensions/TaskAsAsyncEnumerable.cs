
namespace AsyncCollections.Linq;

public static partial class AsyncCollectionsExtensions {
    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this Task<T> task) {
        return new TaskToAsyncEnumerable<T>(task);
    }

    private class TaskToAsyncEnumerable<T> : IAsyncEnumerableOperator<T> {
        private readonly Task<T> item;

        public AsyncExecutionMode ExecutionMode => AsyncExecutionMode.Sequential;

        public TaskToAsyncEnumerable(Task<T> item) {
            this.item = item;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            yield return await this.item;
        }
    }
}
