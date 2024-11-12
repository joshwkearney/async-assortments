namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> AsParallel<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncEnumerableOperator<TSource> op && op.ExecutionMode == AsyncExecutionMode.Parallel) {
            return source;
        }

        return new AsParallelOperator<TSource>(source, AsyncExecutionMode.Parallel);
    }

    public static IAsyncEnumerable<TSource> AsConcurrent<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncEnumerableOperator<TSource> op && op.ExecutionMode == AsyncExecutionMode.Concurrent) {
            return source;
        }

        return new AsParallelOperator<TSource>(source, AsyncExecutionMode.Concurrent);
    }

    public static IAsyncEnumerable<TSource> AsSequential<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncEnumerableOperator<TSource> op && op.ExecutionMode == AsyncExecutionMode.Sequential) {
            return source;
        }

        return new AsParallelOperator<TSource>(source, AsyncExecutionMode.Sequential);
    }

    private class AsParallelOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerable<T> parent;

        public AsyncExecutionMode ExecutionMode { get; }

        public AsParallelOperator(IAsyncEnumerable<T> parent, AsyncExecutionMode mode) {
            this.parent = parent;
            this.ExecutionMode = mode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return this.parent.GetAsyncEnumerator(cancellationToken);
        }
    }
}