//using AsyncCollections.Linq;
//using System.Diagnostics;
//using System.Runtime.CompilerServices;
//using System.Threading.Channels;
//using static AsyncCollections.Reactive.AsyncTimer;

//namespace AsyncCollections.Reactive;

//public static partial class AsyncEnumerableExtensions {
//    public static IAsyncEnumerable<T> Sample<T>(this IAsyncEnumerable<T> sequence, TimeSpan delay) {
//        if (sequence is IAsyncEnumerableOperator<T> collection) {
//            return new SampleOperator<T>(collection, delay);
//        }

//        return SampleHelper(sequence, delay);
//    }

//    public static IAsyncEnumerable<T> Sample<T>(this IAsyncEnumerable<T> sequence, int millis) {
//        return sequence.Sample(TimeSpan.FromMilliseconds(millis));
//    }

//    private static async IAsyncEnumerable<T> SampleHelper<T>(
//        this IAsyncEnumerable<T> sequence,
//        TimeSpan interval,
//        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

//        var channel = Channel.CreateBounded<T>(new BoundedChannelOptions(1) {
//            AllowSynchronousContinuations = true,
//            FullMode = BoundedChannelFullMode.DropOldest
//        });

//        async ValueTask IterateSequence() {
//            try {
//                await foreach (var item in sequence) {
//                    channel.Writer.TryWrite(item);
//                }

//                channel.Writer.Complete();
//            }
//            catch (Exception ex) {
//                channel.Writer.Complete(ex);
//            }
//        }

//        // This is fire and forget on purpose. Exceptions will be handled through the channel
//        var _ = IterateSequence();
//        var last = default(T);

//        // Wait for the first item
//        await channel.Reader.WaitToReadAsync(cancellationToken);

//        while (true) {
//            if (channel.Reader.Completion.IsCompleted) {
//                break;
//            }

//            if (channel.Reader.TryRead(out var item)) {
//                last = item;
//            }
                
//            yield return last!;

//            await Task.Delay(interval, cancellationToken);
//        }
//    }

//    private class SampleOperator<T> : IAsyncEnumerableOperator<T> {
//        private readonly IAsyncEnumerableOperator<T> parent;
//        private readonly TimeSpan delay;

//        public AsyncExecutionMode ExecutionMode { get; }

//        public SampleOperator(IAsyncEnumerableOperator<T> collection, TimeSpan delay) {
//            this.parent = collection;
//            this.delay = delay;
//            this.ExecutionMode = this.parent.ExecutionMode;
//        }

//        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
//            return SampleHelper(this.parent, this.delay).GetAsyncEnumerator(cancellationToken);
//        }
//    }
//}