using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Bypasses a certain number of elements in the sequence and
    ///     returns the remaining elements.
    /// </summary>
    /// <param name="numToSkip">The number of elements to skip.</param>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> containing the elements
    ///     that occur after the skipped elements.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> Skip<TSource>(
        this IAsyncEnumerable<TSource> source, 
        int numToSkip) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (numToSkip < 0) {
            return source;
        }

        if (numToSkip == 0) {
            return source;
        }
        
        // Try to compose with a previous skip or take
        if (source is ISkipTakeOperator<TSource> skipTakeOp) {
            return skipTakeOp.SkipTake(numToSkip, int.MaxValue);
        }

        var pars = source.GetPipelineExecution();

        return new SkipTakeOperator<TSource>(pars, source, numToSkip, int.MaxValue);
    }
}