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

        if (source is IAsyncLinqOperator<TSource> op) {
            if (op.ExecutionMode == AsyncLinqExecutionMode.Parallel) {
                var task = Task.Run(() => elementProducer().AsTask());

                return source.Concat(task.AsAsyncEnumerable());
            }
            else if (op.ExecutionMode == AsyncLinqExecutionMode.Concurrent) {
                return source.Concat(elementProducer().AsAsyncEnumerable());
            }
            else {
                return new AsyncAppendOperator<TSource>(op, elementProducer);
            }
        }

        return AsyncAppendHelper(source, elementProducer);
    }

    private static async IAsyncEnumerable<T> AsyncAppendHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        Func<ValueTask<T>> newItem,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            yield return item;
        }

        yield return await newItem();
    }

    private static async IAsyncEnumerable<T> ConcurrentAsyncAppendHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        Func<ValueTask<T>> newItem,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        var newItemTask = newItem();
        
        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            yield return item;
        }

        yield return await newItemTask;
    }

    private static async IAsyncEnumerable<T> ParallelAsyncAppendHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        Func<ValueTask<T>> newItem,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        var newItemTask = Task.Run(async () => await newItem());

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            yield return item;
        }

        yield return await newItemTask;
    }

    private class AsyncAppendOperator<T> : IAsyncLinqOperator<T> {
        private readonly IAsyncLinqOperator<T> parent;
        private readonly Func<ValueTask<T>> toAppend;

        public AsyncAppendOperator(IAsyncLinqOperator<T> parent, Func<ValueTask<T>> item) {
            this.parent = parent;
            this.toAppend = item;
            this.ExecutionMode = this.parent.ExecutionMode;
        }
        
        public AsyncLinqExecutionMode ExecutionMode { get; }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncLinqExecutionMode.Concurrent) {
                return ConcurrentAsyncAppendHelper(this.parent, this.toAppend).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncLinqExecutionMode.Parallel) {
                return ParallelAsyncAppendHelper(this.parent, this.toAppend).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return AsyncAppendHelper(this.parent, this.toAppend).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}