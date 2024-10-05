using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<E> Select<T, E>(this IAsyncEnumerable<T> sequence, Func<T, E> selector) {
        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new SelectOperator<T, E>(collection, selector);
        }

        return SelectHelper(sequence, selector);
    }

    private static async IAsyncEnumerable<E> SelectHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, E> selector) {
        await foreach (var item in sequence) {
            yield return selector(item);
        }
    }

    private static IAsyncEnumerable<E> ParallelSelectHelper<T, E>(this IAsyncEnumerable<T> sequence, Func<T, E> selector) {
        return sequence.DoParallel<T, E>((item, channel) => {
            channel.Writer.TryWrite(selector(item));

            return ValueTask.CompletedTask;
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