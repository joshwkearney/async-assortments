using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<T> AsyncWhere<T>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask<bool>> selector) {
        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new AsyncWhereOperator<T>(collection, selector);
        }

        return AsyncWhereHelper(sequence, selector);
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

    private class AsyncWhereOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly Func<T, ValueTask<bool>> selector;

        public int Count => -1;

        public AsyncExecutionMode ExecutionMode { get; }

        public AsyncWhereOperator(IAsyncEnumerableOperator<T> collection, Func<T, ValueTask<bool>> selector) {
            this.parent = collection;
            this.selector = selector;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Parallel) {
                return ParallelAsyncWhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncExecutionMode.Concurrent) {
                return ConcurrentAsyncWhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return AsyncWhereHelper(this.parent, this.selector).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}