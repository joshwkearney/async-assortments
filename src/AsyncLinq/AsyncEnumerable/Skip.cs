using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Skip<TSource>(
        this IAsyncEnumerable<TSource> source, 
        int numToSkip) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (numToSkip < 0) {
            throw new ArgumentOutOfRangeException(nameof(numToSkip), "Cannot skip less than zero elements");
        }

        if (numToSkip == 0) {
            return source;
        }
        
        // Try to compose with a previous skip or take
        if (source is ISkipTakeOperator<TSource> skipTakeOp) {
            return skipTakeOp.SkipTake(numToSkip, int.MaxValue);
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new SkipTakeOperator<TSource>(source, numToSkip, int.MaxValue, pars);
    }
}