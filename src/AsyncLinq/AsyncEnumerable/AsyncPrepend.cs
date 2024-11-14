using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> AsyncPrepend<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<ValueTask<TSource>> elementProducer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (elementProducer == null) {
            throw new ArgumentNullException(nameof(elementProducer));
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        if (pars.ExecutionMode == AsyncExecutionMode.Parallel) {
            elementProducer = () => new ValueTask<TSource>(Task.Run(() => elementProducer().AsTask()));
        }

        if (pars.IsUnordered) {
            return elementProducer().AsAsyncEnumerable().Concat(source);
        }
        else {
            return new SequentialAsyncPrependOperator<TSource>(source, elementProducer, pars);
        }
    }

    private class SequentialAsyncPrependOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly Func<ValueTask<T>> newItem;

        public AsyncOperatorParams Params { get; }

        public SequentialAsyncPrependOperator(IAsyncEnumerable<T> parent, Func<ValueTask<T>> item, AsyncOperatorParams pars) {
            this.parent = parent;
            this.newItem = item;
            this.Params = pars;
        }
        
        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            yield return await newItem();

            await foreach (var item in this.parent) {
                yield return item;
            }
        }
    }
}