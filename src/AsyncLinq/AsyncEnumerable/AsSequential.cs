using AsyncCollections.Linq.Operators;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    /// <summary>Instructs asynchronous operators to run sequentially</summary>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
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
    ///         Calling this method is only required if <c>AsConcurrent</c> or <c>AsParallel</c> have 
    ///         previously been called.
    ///     </para>
    /// </remarks>
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    public static IScheduledAsyncEnumerable<TSource> AsSequential<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IScheduledAsyncEnumerable<TSource> s && s.ScheduleMode == AsyncEnumerableScheduleMode.Sequential) {
            return s;
        }
        else if (source is IAsyncOperator<TSource> op) {
            return op.WithExecution(AsyncEnumerableScheduleMode.Sequential);
        }
        else {
            return new WrapperOperator<TSource>(AsyncEnumerableScheduleMode.Sequential, source);
        }
    }
}