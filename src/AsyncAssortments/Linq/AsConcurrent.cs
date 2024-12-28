using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>Instructs asynchronous operators to run concurrently</summary>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <param name="source">The original sequence.</param>
    /// <param name="preserveOrder">
    ///     Determines if asynchronous operations should be returned in the order of the original
    ///     sequence (<c>true</c>), or the order in which they finish (<c>false</c>)
    /// </param>
    /// <remarks>
    ///     <para>
    ///         This method changes the behavior of operators that involve asynchronous operations,
    ///         such as <c>AsyncSelect</c>, <c>AsyncWhere</c>, etc. For these methods, the returned
    ///         tasks are allowed to run at the same time, but they still only use one thread.
    ///     </para>
    ///     
    ///     <para>
    ///         Concurrent execution is best for asynchronous operations that are IO-bound, where
    ///         multithreading is not needed.
    ///     </para>
    /// </remarks>
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    public static IScheduledAsyncEnumerable<TSource> AsConcurrent<TSource>(this IAsyncEnumerable<TSource> source, bool preserveOrder = true) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var pars = preserveOrder ? AsyncEnumerableScheduleMode.ConcurrentOrdered : AsyncEnumerableScheduleMode.ConcurrentUnordered;

        if (source is IAsyncOperator<TSource> op) {
            return op.WithScheduleMode(pars);
        }
        else {
            return new WrapAsyncEnumerableOperator<TSource>(pars, source);
        }
    }
}