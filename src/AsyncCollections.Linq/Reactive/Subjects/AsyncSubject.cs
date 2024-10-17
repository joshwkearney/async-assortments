//using AsyncCollections.Reactive.Subjects;
//using System.Diagnostics;
//using System.Threading.Channels;

//namespace AsyncCollections.Reactive {
//    public sealed class AsyncSubject<T> : IAsyncEnumerableOperator<T>, IAsyncSubject<T> {
//        private readonly List<Subscriber> subscribers = [];
//        private readonly CancellationTokenSource disposeCancellation = new();
//        private readonly int capacity = -1;
//        private bool isDisposed = false;

//        private static readonly UnboundedChannelOptions unboundedOptions = new() {
//            AllowSynchronousContinuations = true,
//            SingleReader = false,
//            SingleWriter = false
//        };

//        AsyncExecutionMode IAsyncEnumerableOperator<T>.ExecutionMode => AsyncExecutionMode.Sequential;

//        public AsyncSubject() { }

//        public AsyncSubject(int subscriberCapacity) {
//            this.capacity = subscriberCapacity;
//        }

//        public void AsyncNext(ValueTask<T> item) {
//            if (this.isDisposed) {
//                throw new ObjectDisposedException("Subject has been disposed, cannot call Next()");
//            }

//            foreach (var subscriber in subscribers) {
//                subscriber.Next(item);
//            }
//        }

//        public Subscriber GetAsyncEnumerator(CancellationToken cancellationToken = default) {
//            Channel<ValueTask<T>> channel;

//            if (this.capacity < 0) {
//                channel = Channel.CreateUnbounded<ValueTask<T>>(unboundedOptions);                
//            }
//            else {
//                channel = Channel.CreateBounded<ValueTask<T>>(new BoundedChannelOptions(this.capacity) {
//                    AllowSynchronousContinuations = true,
//                    FullMode = BoundedChannelFullMode.DropWrite,
//                    SingleReader = false,
//                    SingleWriter = false
//                });
//            }

//            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.disposeCancellation.Token);
//            var subscriber = new Subscriber(this, channel, cancellation.Token);

//            subscribers.Add(subscriber);
//            return subscriber;
//        }

//        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken) {
//            return this.GetAsyncEnumerator(cancellationToken);
//        }

//        public void Dispose() {
//            if (!this.isDisposed) {
//                this.isDisposed = true;
//                this.disposeCancellation.Cancel();
//            }
//        }

//        public class Subscriber : IAsyncEnumerator<T> {
//            private readonly AsyncSubject<T> parent;
//            private readonly CancellationToken cancellationToken;            
//            private readonly Channel<ValueTask<T>> items;

//            public T Current { get; private set; } = default!;

//            internal Subscriber(AsyncSubject<T> parent, Channel<ValueTask<T>> channel, CancellationToken cancellationToken) {
//                this.parent = parent;
//                this.cancellationToken = cancellationToken;
//                this.items = channel;
//            }

//            internal void Next(ValueTask<T> item) {
//                this.items.Writer.TryWrite(item);
//            }

//            public ValueTask DisposeAsync() {
//                parent.subscribers.Remove(this);

//                return ValueTask.CompletedTask;
//            }

//            public async ValueTask<bool> MoveNextAsync() {
//                // If the subject has been disposed, don't wait on more items
//                if (!this.parent.isDisposed) {
//                    try {
//                        await this.items.Reader.WaitToReadAsync(this.cancellationToken);
//                    }
//                    catch (OperationCanceledException) {                        
//                    }
//                }

//                // We should still return items currently in the pipeline though
//                if (!items.Reader.TryRead(out var item)) {
//                    return false;
//                }
                
//                this.Current = await item;
//                return true;
//            }
//        }
//    }
//}
