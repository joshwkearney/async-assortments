using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>
    /// Projects each element of a sequence into a new form.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements of <c>source</c>.</typeparam>
    /// <typeparam name="TResult">The type of the value returned by <c>selector</c>.</typeparam>
    /// <param name="source">A sequence of values to invoke a transform function on.</param>
    /// <param name="selector">A transform function to apply to each source element.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}" /> whose elements are the result of invoking the transform function on each element of source.</returns>
    /// <exception cref="ArgumentNullException"><c>source</c> or <c>selector</c> is <c>null</c>.</exception>
    public static IAsyncEnumerable<TResult> Select<TSource, TResult>(
        this IAsyncEnumerable<TSource> source, 
        Func<TSource, TResult> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        if (source is IAsyncLinqOperator<TSource> collection) {
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

    private class SelectOperator<T, E> : IAsyncLinqOperator<E> {
        private readonly IAsyncLinqOperator<T> parent;
        private readonly Func<T, E> selector;
        
        public AsyncLinqExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public SelectOperator(IAsyncLinqOperator<T> collection, Func<T, E> selector) {
            this.parent = collection;
            this.selector = selector;
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncLinqExecutionMode.Parallel) {
                return ParallelSelectHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return SelectHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}