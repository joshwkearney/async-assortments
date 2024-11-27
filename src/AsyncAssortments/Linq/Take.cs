using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Returns a specified number of contiguous elements from the 
    ///     start of a sequence.
    /// </summary>
    /// <param name="numToTake">The number of elements to take.</param>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> containing the specified
    ///     number of elements from the start of the sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> Take<TSource>(
        this IAsyncEnumerable<TSource> source, 
        int numToTake) {

        if (source == null) {
            new[] { 0 }.Take(8);
            throw new ArgumentNullException(nameof(source));
        }

        if (numToTake < 0) {
            return Empty<TSource>();
        }

        // Try to compose with a previous skip or take
        if (source is ISkipTakeOperator<TSource> skipTakeOp) {
            return skipTakeOp.SkipTake(0, numToTake);
        }

        var pars = source.GetPipelineExecution();

        return new SkipTakeOperator<TSource>(pars, source, 0, numToTake);
    }
}