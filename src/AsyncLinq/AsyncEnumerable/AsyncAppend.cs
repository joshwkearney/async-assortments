using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>Appends a value to the end of the sequence.</summary>
    /// <param name="elementProducer">An async function that returns the element to append.</param>
    /// <returns>A new sequence that ends with the new element.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> AsyncAppend<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<ValueTask<TSource>> elementProducer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (elementProducer == null) {
            throw new ArgumentNullException(nameof(elementProducer));
        }

        // TODO: If the previous operator is an append or concat of an IEnumerable,
        // Replace it instead of tacking on a new one

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        if (pars.ExecutionMode == AsyncExecutionMode.Parallel) {
            elementProducer = () => new ValueTask<TSource>(Task.Run(() => elementProducer().AsTask()));
        }

        if (pars.IsUnordered) {
            return source.Concat(elementProducer().AsAsyncEnumerable());
        }
        else {
            return new SequentialAsyncAppendOperator<TSource>(source, elementProducer, pars);
        }
    }

    private class SequentialAsyncAppendOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly Func<ValueTask<T>> toAppend;

        public AsyncOperatorParams Params { get; }

        public SequentialAsyncAppendOperator(IAsyncEnumerable<T> parent, Func<ValueTask<T>> item, AsyncOperatorParams pars) {
            this.parent = parent;
            this.toAppend = item;
            this.Params = pars;
        }       

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                yield return item;
            }

            yield return await this.toAppend();
        }
    }
}