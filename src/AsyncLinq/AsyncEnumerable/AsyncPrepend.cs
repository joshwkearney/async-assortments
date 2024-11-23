using AsyncCollections.Linq.Operators;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Prepends a value produced by an async function to the end of the sequence.
    /// </summary>
    /// <param name="elementProducer">An async function that returns the element to prepend.</param>
    /// <returns>A new sequence that starts with the new element.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <remarks>
    ///     This is an async operator and can run sequentially, concurrently, or in parallel,
    ///     depending on the execution mode of the input sequence. See the <c>.AsSequential()</c>,
    ///     <c>.AsConcurrent()</c>, and <c>.AsParallel()</c> operators for details
    /// </remarks>
    /// <seealso cref="Prepend{TSource}(IAsyncEnumerable{TSource}, TSource)" />
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    public static IAsyncEnumerable<TSource> AsyncPrepend<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<CancellationToken, ValueTask<TSource>> elementProducer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (elementProducer == null) {
            throw new ArgumentNullException(nameof(elementProducer));
        }

        var pars = source.GetPipelineExecution();
        
        return new SingletonOperator<TSource>(pars, elementProducer).Concat(source);
    }

    /// <inheritdoc cref="AsyncPrepend{TSource}(IAsyncEnumerable{TSource}, Func{CancellationToken, ValueTask{TSource}})" />
    public static IAsyncEnumerable<TSource> AsyncPrepend<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<ValueTask<TSource>> elementProducer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (elementProducer == null) {
            throw new ArgumentNullException(nameof(elementProducer));
        }

        var pars = source.GetPipelineExecution();
        
        return new SingletonOperator<TSource>(pars, _ => elementProducer()).Concat(source);
    }
}