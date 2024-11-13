namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>Instructs asynchronous operators to run in parallel on the thread pool.</summary>
    /// <remarks>
    ///     <para>
    ///         This method changes the behavior of operators that involve asynchronous operations,
    ///         such as <c>AsyncSelect</c>, <c>AsyncWhere</c>, etc. For these methods, the returned
    ///         tasks are scheduled to run at the same time on the thread pool.
    ///     </para>
    ///     
    ///     <para>
    ///         Parallel execution is best for asynchronous operations that are CPU-bound where
    ///         multithreading is needed.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> AsParallel<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncLinqOperator<TSource> op && op.ExecutionMode == AsyncLinqExecutionMode.Parallel) {
            return source;
        }

        return new AsParallelOperator<TSource>(source, AsyncLinqExecutionMode.Parallel);
    }

    /// <summary>Instructs asynchronous operators to run concurrently</summary>
    /// <remarks>
    ///     <para>
    ///         This method changes the behavior of operators that involve asynchronous operations,
    ///         such as <c>AsyncSelect</c>, <c>AsyncWhere</c>, etc. For these methods, the returned
    ///         tasks are allowed to run at the same time, but are not scheduled to run on the thread
    ///         pool.
    ///     </para>
    ///     
    ///     <para>
    ///         Concurrent execution is best for asynchronous operations that are IO-bound where
    ///         multithreading is not needed.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> AsConcurrent<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncLinqOperator<TSource> op && op.ExecutionMode == AsyncLinqExecutionMode.Concurrent) {
            return source;
        }

        return new AsParallelOperator<TSource>(source, AsyncLinqExecutionMode.Concurrent);
    }

    /// <summary>
    /// Instructs asynchronous operators to run sequentially
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method changes the behavior of operators that involve asynchronous operations,
    ///         such as <c>AsyncSelect</c>, <c>AsyncWhere</c>, etc. For these methods, the returned
    ///         tasks are run one after another, with later tasks only starting once previous tasks
    ///         have finished.
    ///     </para>
    ///     
    ///     <para>
    ///         Sequential execution is the default behavior of <see cref="IAsyncEnumerable{T}" />.
    ///         Calling this method is only required if 
    ///         <see cref="AsConcurrent{TSource}(IAsyncEnumerable{TSource})"/> or 
    ///         <see cref="AsParallel{TSource}(IAsyncEnumerable{TSource})" /> have previously been called.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
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