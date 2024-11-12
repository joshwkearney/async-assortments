using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> AsyncWhere<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        if (source is IAsyncLinqOperator<TSource> collection) {
            return new AsyncWhereOperator<TSource>(collection, predicate);
        }

        return AsyncWhereHelper(source, predicate);
    }

    private static async IAsyncEnumerable<T> AsyncWhereHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        Func<T, ValueTask<bool>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            if (await selector(item)) {
                yield return item;
            }
        }
    }

    private static IAsyncEnumerable<T> ConcurrentAsyncWhereHelper<T>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<bool>> selector) {
        return sequence.DoConcurrent<T, T>(async (item, channel) => {
            if (await selector(item)) {
                await channel.Writer.WriteAsync(item);
            }
        });
    }

    private static IAsyncEnumerable<T> ParallelAsyncWhereHelper<T>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<bool>> selector) {
        return sequence.DoParallel<T, T>(async (item, channel) => {
            if (await selector(item)) {
                await channel.Writer.WriteAsync(item);
            }
        });
    }

    private class AsyncWhereOperator<T> : IAsyncLinqOperator<T> {
        private readonly IAsyncLinqOperator<T> parent;
        private readonly Func<T, ValueTask<bool>> selector;

        public int Count => -1;

        public AsyncLinqExecutionMode ExecutionMode { get; }

        public AsyncWhereOperator(IAsyncLinqOperator<T> collection, Func<T, ValueTask<bool>> selector) {
            this.parent = collection;
            this.selector = selector;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncLinqExecutionMode.Parallel) {
                return ParallelAsyncWhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncLinqExecutionMode.Concurrent) {
                return ConcurrentAsyncWhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return AsyncWhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}