namespace AsyncAssortments {
    /// <summary>
    ///     Represents an <see cref="IAsyncEnumerable{T}" /> that will schedule async operators in a particular way.
    ///     See <see cref="AsyncEnumerableScheduleMode" /> for available schedule modes.
    /// </summary>
    /// <inheritdoc cref="IAsyncEnumerable{T}" />
    public interface IScheduledAsyncEnumerable<out T> : IAsyncEnumerable<T> {
        /// <summary>
        ///     The scheduling used to run operators on this <see cref="IAsyncEnumerable{T}" />.
        /// </summary>
        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        /// <summary>
        ///     The maximum number of tasks that are allowed to execute simultaneously.
        /// </summary>
        public int MaxConcurrency { get; }
    }

    /// <summary>
    ///     Represents different ways of running async operators that are applied to an
    ///     <see cref="IScheduledAsyncEnumerable{T}" />.
    /// </summary>
    public enum AsyncEnumerableScheduleMode {
        /// <summary>Async operators will run sequentially as needed.</summary>
        Sequential,
        
        /// <summary>
        ///     Async operators will run concurrently, but the results will maintain the order of the original
        ///     sequence. All operations run on only one thread.
        /// </summary>
        ConcurrentOrdered,
        
        /// <summary>
        ///     Async operators will run concurrently, and the results will be yielded in the order in which they
        ///     finish; the ordering of the original sequence is not maintained. All operations run on only one thread.
        /// </summary>
        ConcurrentUnordered,
        
        /// <summary>
        ///     Async operators will run concurrently, but the results will maintain the order of the original
        ///     sequence. Operations will be run on the thread pool.
        /// </summary>
        ParallelOrdered,
        
        /// <summary>
        ///     Async operators will run concurrently, and the results will be yielded in the order in which they
        ///     finish; the ordering of the original sequence is not maintained. Operations will be run on the thread
        ///     pool.
        /// </summary>
        ParallelUnordered
    }

    internal static class AsyncPipelineExecutionExtensions {
        internal static int GetMaxConcurrency<T>(this IAsyncEnumerable<T> source) {
            var concurrency = -1;

            if (source is IScheduledAsyncEnumerable<T> pipeline) {
                concurrency = pipeline.MaxConcurrency;
            }

            return concurrency;
        }

        internal static AsyncEnumerableScheduleMode GetScheduleMode<T>(this IAsyncEnumerable<T> source) {
            var execution = AsyncEnumerableScheduleMode.Sequential;

            if (source is IScheduledAsyncEnumerable<T> pipeline) {
                execution = pipeline.ScheduleMode;
            }

            return execution;
        }

        internal static bool IsOrdered(this AsyncEnumerableScheduleMode pars) {
            return pars == AsyncEnumerableScheduleMode.Sequential
                || pars == AsyncEnumerableScheduleMode.ConcurrentOrdered
                || pars == AsyncEnumerableScheduleMode.ParallelOrdered;
        }

        internal static bool IsUnordered(this AsyncEnumerableScheduleMode pars) {
            return pars == AsyncEnumerableScheduleMode.ConcurrentUnordered
                || pars == AsyncEnumerableScheduleMode.ParallelUnordered;
        }

        internal static bool IsParallel(this AsyncEnumerableScheduleMode pars) {
            return pars == AsyncEnumerableScheduleMode.ParallelOrdered
                || pars == AsyncEnumerableScheduleMode.ParallelUnordered;
        }

        internal static AsyncEnumerableScheduleMode MakeConcurrent(this AsyncEnumerableScheduleMode pars) {
            if (pars.IsUnordered()) {
                return AsyncEnumerableScheduleMode.ConcurrentUnordered;
            }
            else {
                return AsyncEnumerableScheduleMode.ConcurrentOrdered;
            }
        }

        internal static AsyncEnumerableScheduleMode MakeParallel(this AsyncEnumerableScheduleMode pars) {
            if (pars.IsUnordered()) {
                return AsyncEnumerableScheduleMode.ParallelUnordered;
            }
            else {
                return AsyncEnumerableScheduleMode.ParallelOrdered;
            }
        }

        internal static AsyncEnumerableScheduleMode MakeOrdered(this AsyncEnumerableScheduleMode pars) {
            if (pars == AsyncEnumerableScheduleMode.ConcurrentUnordered) {
                return AsyncEnumerableScheduleMode.ConcurrentOrdered;
            }
            else if (pars == AsyncEnumerableScheduleMode.ParallelUnordered) {
                return AsyncEnumerableScheduleMode.ParallelOrdered;
            }
            else {
                return pars;
            }
        }

        internal static AsyncEnumerableScheduleMode MakeUnordered(this AsyncEnumerableScheduleMode pars) {
            if (pars == AsyncEnumerableScheduleMode.ConcurrentOrdered) {
                return AsyncEnumerableScheduleMode.ConcurrentUnordered;
            }
            else if (pars == AsyncEnumerableScheduleMode.ParallelOrdered) {
                return AsyncEnumerableScheduleMode.ParallelUnordered;
            }
            else {
                return pars;
            }
        }
    }
}
