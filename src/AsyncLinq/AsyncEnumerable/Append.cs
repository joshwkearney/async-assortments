using System.Runtime.CompilerServices;

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

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        if (pars.IsUnordered) {
            return source.Concat([element]);
        }
        else {
            return new SequentialAppendOperator<TSource>(source, element, pars);
        }
    }

    private class SequentialAppendOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly T toAppend;

        public AsyncOperatorParams Params { get; }

        public SequentialAppendOperator(IAsyncEnumerable<T> parent, T item, AsyncOperatorParams pars) {
            this.parent = parent;
            this.toAppend = item;
            this.Params = pars;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                yield return item;
            }

            yield return this.toAppend;
        }
    }
}