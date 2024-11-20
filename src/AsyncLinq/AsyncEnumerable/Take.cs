using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Take<TSource>(
        this IAsyncEnumerable<TSource> source, 
        int numToTake) {

        if (source == null) {
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