using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
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
    public static IAsyncPipeline<TSource> AsSequential<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncOperator<TSource> op) {
            return op.WithExecution(AsyncPipelineExecution.Sequential);
        }
        else {
            return new WrapperOperator<TSource>(AsyncPipelineExecution.Sequential, source);
        }
    }
}