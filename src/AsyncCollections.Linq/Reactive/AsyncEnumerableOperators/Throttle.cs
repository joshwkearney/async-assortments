//using AsyncCollections;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Runtime.CompilerServices;
//using System.Text;
//using System.Threading.Channels;
//using System.Threading.Tasks;

//namespace CollectionTesting;

//public static partial class AsyncEnumerableExtensions {
//    public static IAsyncEnumerable<T> Throttle<T>(this IAsyncEnumerable<T> sequence, TimeSpan window) {
//        if (sequence is IAsyncEnumerableOperator<T> collection) {
//            return new ThrottleOperator<T>(collection, window);
//        }

//        return ThrottleHelper(sequence, window);
//    }

//    public static IAsyncEnumerable<T> Throttle<T>(this IAsyncEnumerable<T> sequence, int millis) {
//        return sequence.Throttle(TimeSpan.FromMilliseconds(millis));
//    }

//    private static async IAsyncEnumerable<T> ThrottleHelper<T>(
//        this IAsyncEnumerable<T> sequence,
//        TimeSpan interval,
//        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

//        var throttleChannel = Channel.CreateBounded<T>(new BoundedChannelOptions(1) {
//            AllowSynchronousContinuations = true,
//            FullMode = BoundedChannelFullMode.DropOldest
//        });

//        async ValueTask IterateSequence() {
//            try {
//                await foreach (var item in sequence) {
//                    throttleChannel.Writer.TryWrite(item);
//                }

//                throttleChannel.Writer.Complete();
//            }
//            catch (Exception ex) {
//                throttleChannel.Writer.Complete(ex);
//            }
//        }

//        // This is fire and forget on purpose. Exceptions will be handled through the channel
//        var _ = IterateSequence();

//        var watch = new Stopwatch();
//        watch.Start();

//        while (true) {
//            // Wait for something to read
//            var hasMore = await throttleChannel.Reader.WaitToReadAsync(cancellationToken);

//            if (!hasMore) {
//                break;
//            }

//            if (!throttleChannel.Reader.TryRead(out var item)) {
//                break;
//            }

//            if (watch.Elapsed > interval) {
//                yield return item;
//            }

//            watch.Restart();
//        }
//    }

//    private class ThrottleOperator<T> : IAsyncEnumerableOperator<T> {
//        private readonly IAsyncEnumerableOperator<T> parent;
//        private readonly TimeSpan delay;

//        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

//        public ThrottleOperator(IAsyncEnumerableOperator<T> collection, TimeSpan delay) {
//            this.parent = collection;
//            this.delay = delay;
//        }

//        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
//            return ThrottleHelper(this.parent, this.delay).GetAsyncEnumerator(cancellationToken);
//        }
//    }
//}