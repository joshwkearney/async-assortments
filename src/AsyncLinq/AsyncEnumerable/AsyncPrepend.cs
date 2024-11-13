using System.Runtime.CompilerServices;

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

        if (source is IAsyncLinqOperator<TSource> op) {
            if (op.ExecutionMode == AsyncLinqExecutionMode.Parallel) {
                var task = Task.Run(() => elementProducer().AsTask());

                return task.AsAsyncEnumerable().Concat(source);
            }
            else if (op.ExecutionMode == AsyncLinqExecutionMode.Concurrent) {
                return elementProducer().AsAsyncEnumerable().Concat(source);
            }
            else {
                return new AsyncPrependOperator<TSource>(op, elementProducer);
            }
        }

        return AsyncPrependHelper(source, elementProducer);
    }

    private static async IAsyncEnumerable<T> AsyncPrependHelper<T>(
        this IAsyncEnumerable<T> sequence,
        Func<ValueTask<T>> newItem,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        yield return await newItem();

        await foreach (var item in sequence) {
            yield return item;
        }
    }

    private class AsyncPrependOperator<T> : IAsyncLinqOperator<T> {
        private readonly IAsyncLinqOperator<T> parent;
        private readonly Func<ValueTask<T>> newItem;

        public AsyncPrependOperator(IAsyncLinqOperator<T> parent, Func<ValueTask<T>> item) {
            this.parent = parent;
            this.newItem = item;
        }
        
        public AsyncLinqExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return AsyncPrependHelper(this.parent, this.newItem).GetAsyncEnumerator(cancellationToken);
        }
    }
}