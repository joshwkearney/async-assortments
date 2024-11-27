using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>Appends a value produced by an async function to the end of the sequence.</summary>
    /// <param name="elementProducer">An async function that returns the element to append.</param>
    /// <returns>A new sequence that ends with the new element.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <remarks>
    ///     This is an async operator and can run sequentially, concurrently, or in parallel,
    ///     depending on the execution mode of the input sequence. See the <c>.AsSequential()</c>,
    ///     <c>.AsConcurrent()</c>, and <c>.AsParallel()</c> operators for details
    /// </remarks>
    /// <seealso cref="Append{TSource}(IAsyncEnumerable{TSource}, TSource)" />
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    public static IAsyncEnumerable<TSource> AsyncAppend<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<CancellationToken, ValueTask<TSource>> elementProducer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (elementProducer == null) {
            throw new ArgumentNullException(nameof(elementProducer));
        }

        var pars = source.GetPipelineExecution();
        
        return source.Concat(new WrapAsyncFuncOperator<TSource>(pars, elementProducer));
    }

    /// <inheritdoc cref="AsyncAppend{TSource}(IAsyncEnumerable{TSource}, Func{CancellationToken, ValueTask{TSource}})" />
    public static IAsyncEnumerable<TSource> AsyncAppend<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<ValueTask<TSource>> elementProducer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (elementProducer == null) {
            throw new ArgumentNullException(nameof(elementProducer));
        }

        var pars = source.GetPipelineExecution();
        
        return source.Concat(new WrapAsyncFuncOperator<TSource>(pars, _ => elementProducer()));
    }
}