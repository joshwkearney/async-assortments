using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>Instructs asynchronous operators to run in parallel on the thread pool.</summary>
    /// <param name="preserveOrder">
    ///     Determines if asynchronous operations should be returned in the order of the original
    ///     sequence (<c>true</c>), or the order in which they finish (<c>false</c>)
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
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> AsParallel<TSource>(this IAsyncEnumerable<TSource> source, bool preserveOrder = true) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var pars = new AsyncOperatorParams(AsyncExecutionMode.Parallel, !preserveOrder);

        if (source is IAsyncOperator<TSource> op && op.Params == pars) {
            return op;
        }

        if (source is ParamsChangeOperator<TSource> changeOp) {
            return new ParamsChangeOperator<TSource>(changeOp.Parent, pars);
        }

        return new ParamsChangeOperator<TSource>(source, pars);
    }

    /// <summary>Instructs asynchronous operators to run concurrently</summary>
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
    ///         Concurrent execution is best for asynchronous operations that are IO-bound where
    ///         multithreading is not needed.
    ///     </para>
    /// </remarks>
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> AsConcurrent<TSource>(this IAsyncEnumerable<TSource> source, bool preserveOrder = true) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var pars = new AsyncOperatorParams(AsyncExecutionMode.Concurrent, !preserveOrder);

        if (source is IAsyncOperator<TSource> op && op.Params == pars) {
            return op;
        }

        if (source is ParamsChangeOperator<TSource> changeOp) {
            return new ParamsChangeOperator<TSource>(changeOp.Parent, pars);
        }

        return new ParamsChangeOperator<TSource>(source, pars);
    }

    /// <summary>Instructs asynchronous operators to run sequentially</summary>
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

        var pars = new AsyncOperatorParams(AsyncExecutionMode.Sequential, false);

        if (source is IAsyncOperator<TSource> op && op.Params == pars) {
            return op;
        }

        if (source is ParamsChangeOperator<TSource> changeOp) {
            return new ParamsChangeOperator<TSource>(changeOp.Parent, pars);
        }

        return new ParamsChangeOperator<TSource>(source, pars);
    }

    private class ParamsChangeOperator<T> : IAsyncOperator<T> {
        public IAsyncEnumerable<T> Parent { get; }

        public AsyncOperatorParams Params { get; }

        public ParamsChangeOperator(IAsyncEnumerable<T> parent, AsyncOperatorParams pars) {
            this.Parent = parent;
            this.Params = pars;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return this.Parent.GetAsyncEnumerator(cancellationToken);
        }
    }
}