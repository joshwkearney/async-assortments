using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Prepend<TSource>(
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
            return new SequentialPrependOperator<TSource>(source, element, pars);
        }
    }

    private class SequentialPrependOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly T newItem;

        public AsyncOperatorParams Params { get; }

        public SequentialPrependOperator(IAsyncEnumerable<T> parent, T item, AsyncOperatorParams pars) {
            this.parent = parent;
            this.newItem = item;
            this.Params = pars;
        }
       
        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            // Create the iterator first so subscribe works correctly
            var iterator = this.parent.WithCancellation(cancellationToken).GetAsyncEnumerator();
            var moveNextTask = iterator.MoveNextAsync();

            // Then yield the prepended item
            cancellationToken.ThrowIfCancellationRequested();
            yield return newItem;

            // Then yield the rest
            while (await moveNextTask) {
                yield return iterator.Current;
                moveNextTask = iterator.MoveNextAsync();
            }
        }
    }
}