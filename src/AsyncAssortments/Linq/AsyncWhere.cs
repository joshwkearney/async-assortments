using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Filters a sequence using an async predicate to select elements.
    /// </summary>
    /// <param name="source">The sequence to filter</param>
    /// <param name="predicate">An async predicate to filter each element</param>
    /// <returns>A new sequence containing only items that match the predicate.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    /// <remarks>
    ///     This is an async operator and can run sequentially, concurrently, or in parallel,
    ///     depending on the execution mode of the input sequence. See the <c>.AsSequential()</c>,
    ///     <c>.AsConcurrent()</c>, and <c>.AsParallel()</c> operators for details
    /// </remarks>
    /// <seealso cref="Where{TSource}(IAsyncEnumerable{TSource}, Func{TSource, bool})"/>
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    public static IAsyncEnumerable<TSource> AsyncWhere<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, CancellationToken, ValueTask<bool>> predicate) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        // First try to compose this operation with a previous operator
        if (source is IAsyncWhereOperator<TSource> whereOp) {
            return whereOp.AsyncWhere(predicate);
        }

        var pars = source.GetScheduleMode();

        return new SelectWhereTaskOperator<TSource, TSource>(pars, source, async (x, c) => new(await predicate(x, c), x));
    }
    
    /// <inheritdoc cref="AsyncWhere{TSource}(IAsyncEnumerable{TSource}, Func{TSource, CancellationToken, ValueTask{bool}})" />
    public static IAsyncEnumerable<TSource> AsyncWhere<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        return source.AsyncWhere((x, c) => predicate(x));
    }
}