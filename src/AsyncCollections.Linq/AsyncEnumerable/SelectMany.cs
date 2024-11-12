using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, IAsyncEnumerable<TResult>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        if (source is IAsyncEnumerableOperator<TSource> op) {
            return new SelectManyAsyncOperator<TSource, TResult>(op, selector);
        }

        return SelectManyHelper(source, selector);
    }

    public static IAsyncEnumerable<TResult> SelectMany<TSource, TResult>(
        this IAsyncEnumerable<TSource> sequence, 
        Func<TSource, IEnumerable<TResult>> selector) {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        return sequence.SelectMany(x => selector(x).AsAsyncEnumerable());
    }

    //public static IAsyncEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(
    //    this IAsyncEnumerable<TSource> sequence,
    //    Func<TSource, IEnumerable<TCollection>> selector,
    //    Func<TSource, TCollection, TResult> resultSelector) {

    //    return sequence.SelectMany(x => selector(x).AsAsyncEnumerable()).Select();
    //}

    private static async IAsyncEnumerable<E> SelectManyHelper<T, E>(
        this IAsyncEnumerable<T> sequence, 
        Func<T, IAsyncEnumerable<E>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            await foreach (var sub in selector(item).WithCancellation(cancellationToken)) {
                yield return sub;
            }
        }
    }

    private static IAsyncEnumerable<E> ParallelSelectManyHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
        return sequence.DoParallel<T, E>(async (item, channel) => {
            await foreach (var sub in selector(item)) {
                await channel.Writer.WriteAsync(sub);
            }
        });
    }

    private static IAsyncEnumerable<E> ConcurrentSelectManyHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
        return sequence.DoConcurrent<T, E>(async (item, channel) => {
            await foreach (var sub in selector(item)) {
                await channel.Writer.WriteAsync(sub);
            }
        });
    }

    private class SelectManyAsyncOperator<T, E> : IAsyncEnumerableOperator<E> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<T, IAsyncEnumerable<E>> selector;
        
        public AsyncExecutionMode ExecutionMode { get; }

        public SelectManyAsyncOperator(IAsyncEnumerableOperator<T> sequence, Func<T, IAsyncEnumerable<E>> selector) {
            this.parent = sequence;
            this.selector = selector;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Parallel) {
                return ParallelSelectManyHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncExecutionMode.Concurrent) {
                return ConcurrentSelectManyHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return SelectManyHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}