using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>Runs an async function for each element in a sequence.</summary>
    /// <param name="source">The original sequence.</param>
    /// <param name="selector">A function to run for each source element.</param>
    /// <returns>
    ///     An <see cref="ValueTask" /> that completes when the entire sequence
    ///     has been enumerated.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <remarks>
    ///     This is an async operator and can run sequentially, concurrently, or in parallel,
    ///     depending on the execution mode of the input sequence. See the <c>.AsSequential()</c>,
    ///     <c>.AsConcurrent()</c>, and <c>.AsParallel()</c> operators for details.
    /// </remarks>
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    public static ValueTask ForEachAsync<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, ValueTask> selector) {
        
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        return source.ForEachAsync((x, _) => selector(x));
    }
    
    /// <summary>Runs an async function for each element in a sequence.</summary>
    /// <param name="source">The original sequence.</param>
    /// <param name="selector">A function to run for each source element.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>
    ///     An <see cref="ValueTask" /> that completes when the entire sequence
    ///     has been enumerated.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <remarks>
    ///     This is an async operator and can run sequentially, concurrently, or in parallel,
    ///     depending on the execution mode of the input sequence. See the <c>.AsSequential()</c>,
    ///     <c>.AsConcurrent()</c>, and <c>.AsParallel()</c> operators for details.
    /// </remarks>
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    public static ValueTask ForEachAsync<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, CancellationToken, ValueTask> selector,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        return Helper();
        
        async ValueTask Helper() {
            // We're using AsyncSelect because it can handle sequential, concurrent,
            // and parallel sequences correctly
            var seq = source.AsyncSelect(async (x, c) => {
                await selector(x, c);
                return true;
            });

            await foreach (var _ in seq.WithCancellation(cancellationToken)) { }
        }
    }
    
    /// <summary>Runs a function for each element in a sequence.</summary>
    /// <param name="source">The original sequence.</param>
    /// <param name="selector">A function to run for each source element.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>
    ///     An <see cref="ValueTask" /> that completes when the entire sequence
    ///     has been enumerated.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <remarks>
    ///     This is an async operator and can run sequentially, concurrently, or in parallel,
    ///     depending on the execution mode of the input sequence. See the <c>.AsSequential()</c>,
    ///     <c>.AsConcurrent()</c>, and <c>.AsParallel()</c> operators for details.
    /// </remarks>
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    public static ValueTask ForEachAsync<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Action<TSource> selector,
        CancellationToken cancellationToken = default) {
        
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        // Note: We're calling the async version of foreach because it handles parallel
        // execution correctly
        return source.ForEachAsync(
            (x, c) => { 
                selector(x);
                return new ValueTask();
            }, 
            cancellationToken);
    }
}