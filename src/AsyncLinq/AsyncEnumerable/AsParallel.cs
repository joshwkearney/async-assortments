namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> AsParallel<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncLinqOperator<TSource> op && op.ExecutionMode == AsyncLinqExecutionMode.Parallel) {
            return source;
        }

        return new AsParallelOperator<TSource>(source, AsyncLinqExecutionMode.Parallel);
    }

    public static IAsyncEnumerable<TSource> AsConcurrent<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncLinqOperator<TSource> op && op.ExecutionMode == AsyncLinqExecutionMode.Concurrent) {
            return source;
        }

        return new AsParallelOperator<TSource>(source, AsyncLinqExecutionMode.Concurrent);
    }

    public static IAsyncEnumerable<TSource> AsSequential<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncLinqOperator<TSource> op && op.ExecutionMode == AsyncLinqExecutionMode.Sequential) {
            return source;
        }

        return new AsParallelOperator<TSource>(source, AsyncLinqExecutionMode.Sequential);
    }

    private class AsParallelOperator<T> : IAsyncLinqOperator<T> {
        private readonly IAsyncEnumerable<T> parent;

        public AsyncLinqExecutionMode ExecutionMode { get; }

        public AsParallelOperator(IAsyncEnumerable<T> parent, AsyncLinqExecutionMode mode) {
            this.parent = parent;
            this.ExecutionMode = mode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return this.parent.GetAsyncEnumerator(cancellationToken);
        }
    }
}