using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> Delay<T>(this IAsyncEnumerable<T> sequence, TimeSpan delay) {
        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new DelayOperator<T>(collection, delay);
        }

        return DelayHelper(sequence, delay);
    }

    public static IAsyncEnumerable<T> Delay<T>(this IAsyncEnumerable<T> sequence, int millis) {
        return sequence.Delay(TimeSpan.FromMilliseconds(millis));
    }

    private static async IAsyncEnumerable<T> DelayHelper<T>(this IAsyncEnumerable<T> sequence, TimeSpan delay) {
        await foreach (var item in sequence) {
            await Task.Delay(delay);

            yield return item;
        }
    }

    private static IAsyncEnumerable<T> ConcurrentDelayHelper<T>(this IAsyncEnumerable<T> sequence, TimeSpan delay) {
        return sequence.DoConcurrent<T, T>(async (item, channel) => {
            await Task.Delay(delay);

            channel.Writer.TryWrite(item);
        });
    }

    private class DelayOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly TimeSpan delay;

        public int Count => this.parent.Count;

        public AsyncExecutionMode ExecutionMode { get; }

        public DelayOperator(IAsyncEnumerableOperator<T> collection, TimeSpan delay) {
            this.parent = collection;
            this.delay = delay;
            this.ExecutionMode = this.parent.ExecutionMode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Parallel || this.ExecutionMode == AsyncExecutionMode.Concurrent) {
                return ConcurrentDelayHelper(this.parent, this.delay).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return DelayHelper(this.parent, this.delay).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}