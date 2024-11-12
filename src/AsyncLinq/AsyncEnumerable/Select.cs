using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, TResult> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        if (source is IAsyncEnumerableOperator<TSource> collection) {
            return new SelectOperator<TSource, TResult>(collection, selector);
        }

        return SelectHelper(source, selector);
    }

    private static async IAsyncEnumerable<E> SelectHelper<T, E>(
        this IAsyncEnumerable<T> sequence, 
        Func<T, E> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            yield return selector(item);
        }
    }

    private static IAsyncEnumerable<E> ParallelSelectHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, E> selector) {
        return sequence.DoParallel<T, E>(async (item, channel) => {
            await channel.Writer.WriteAsync(selector(item));
        });
    }

    private class SelectOperator<T, E> : IAsyncEnumerableOperator<E> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<T, E> selector;
        
        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public SelectOperator(IAsyncEnumerableOperator<T> collection, Func<T, E> selector) {
            this.parent = collection;
            this.selector = selector;
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Parallel) {
                return ParallelSelectHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return SelectHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}