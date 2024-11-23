using AsyncCollections.Linq.Operators;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Transforms the input sequence by applying an async function to each element.
    /// </summary>
    /// <param name="source">The sequence to transform</param>
    /// <param name="selector">An async function to apply to each element</param>
    /// <returns>A new sequence that is a result of the projection.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    /// <remarks>
    ///     This is an async operator and can run sequentially, concurrently, or in parallel,
    ///     depending on the execution mode of the input sequence. See the <c>.AsSequential()</c>,
    ///     <c>.AsConcurrent()</c>, and <c>.AsParallel()</c> operators for details
    /// </remarks>
    /// <seealso cref="Select{TSource, TResult}(IAsyncEnumerable{TSource}, Func{TSource, TResult})"/>
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    public static IAsyncEnumerable<TResult> AsyncSelect<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, CancellationToken, ValueTask<TResult>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        // First try to compose this operation with a previous operator
        if (source is IScheduledAsyncSelectOperator<TSource> selectOp) {
            return selectOp.AsyncSelect(selector);
        }

        var pars = source.GetPipelineExecution();

        return new SelectWhereTaskOperator<TSource, TResult>(
            pars, 
            source, 
            async (x, c) => new(true, await selector(x, c)));
    }
    
    /// <inheritdoc cref="AsyncSelect{TSource, TResult}(IAsyncEnumerable{TSource}, Func{TSource, CancellationToken, ValueTask{TResult}})" />
    public static IAsyncEnumerable<TResult> AsyncSelect<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, ValueTask<TResult>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        return source.AsyncSelect((x, _) => selector(x));
    }
}