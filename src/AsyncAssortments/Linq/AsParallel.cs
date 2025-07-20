using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>Instructs asynchronous operators to run in parallel on the thread pool.</summary>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <param name="source">The original sequence.</param>
    /// <param name="preserveOrder">
    ///     Determines if asynchronous operations should be returned in the order of the original
    ///     sequence (<c>true</c>), or the order in which they finish (<c>false</c>)
    /// </param>
    /// <param name="maxConcurrency">
    ///    The maximum number of tasks that are allowed to execute simultaneously.
    /// </param>
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
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    public static IScheduledAsyncEnumerable<TSource> AsParallel<TSource>(this IAsyncEnumerable<TSource> source, bool preserveOrder = true, int maxConcurrency = -1) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var pars = preserveOrder ? AsyncEnumerableScheduleMode.ParallelOrdered : AsyncEnumerableScheduleMode.ParallelUnordered;

        if (source is IAsyncOperator<TSource> op) {
            return op.WithScheduleMode(pars, maxConcurrency);
        }
        else {
            return new WrapAsyncEnumerableOperator<TSource>(pars, maxConcurrency, source);
        }
    }
}