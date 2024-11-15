using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>Appends a value to the end of the sequence.</summary>
    /// <param name="element">The value to append to the source sequence.</param>
    /// <returns>A new sequence that ends with the new element.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> Append<TSource>(
        this IAsyncEnumerable<TSource> source, 
        TSource element) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IConcatEnumerablesOperator<TSource> concatOp) {
            return concatOp.ConcatEnumerables([], [element]);
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new Operators.ConcatEnumerablesOperator<TSource>(source, [], [element], pars);
    }
}