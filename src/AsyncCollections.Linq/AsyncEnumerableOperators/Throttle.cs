using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> Throttle<T>(this IAsyncEnumerable<T> sequence, TimeSpan window) {
        if (sequence is IAsyncEnumerableOperator<T> collection) {
            return new ThrottleOperator<T>(collection, window);
        }

        return ThrottleHelper(sequence, window);
    }

    public static IAsyncEnumerable<T> Throttle<T>(this IAsyncEnumerable<T> sequence, int millis) {
        return sequence.Throttle(TimeSpan.FromMilliseconds(millis));
    }

    private static async IAsyncEnumerable<T> ThrottleHelper<T>(this IAsyncEnumerable<T> sequence, TimeSpan delay) {
        var watch = new Stopwatch();

        await foreach (var item in sequence) {
            if (!watch.IsRunning || watch.Elapsed > delay) {
                watch.Restart();

                yield return item;
            }
        }

        watch.Stop();
    }

    private class ThrottleOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly TimeSpan delay;
        
        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public ThrottleOperator(IAsyncEnumerableOperator<T> collection, TimeSpan delay) {
            this.parent = collection;
            this.delay = delay;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return ThrottleHelper(this.parent, this.delay).GetAsyncEnumerator(cancellationToken);
        }
    }
}