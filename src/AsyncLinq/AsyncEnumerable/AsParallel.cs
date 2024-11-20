﻿using AsyncLinq.Operators;

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
    public static IAsyncPipeline<TSource> AsParallel<TSource>(this IAsyncEnumerable<TSource> source, bool preserveOrder = true) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var pars = preserveOrder ? AsyncPipelineExecution.ParallelOrdered : AsyncPipelineExecution.ParallelUnordered;

        if (source is IAsyncOperator<TSource> op) {
            return op.WithExecution(pars);
        }
        else {
            return new WrapperOperator<TSource>(pars, source);
        }
    }
}