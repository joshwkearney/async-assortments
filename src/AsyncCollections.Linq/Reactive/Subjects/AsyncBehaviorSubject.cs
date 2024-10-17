//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Channels;
//using System.Threading.Tasks;

//namespace AsyncCollections.Reactive.Subjects {
//    public sealed class AsyncBehaviorSubject<T> : IAsyncEnumerableOperator<T>, IDisposable {
//        private readonly List<Subscriber> subscribers = [];
//        private readonly Task<T> first;
//        private readonly CancellationTokenSource disposeCancellation = new();
//        private bool isDisposed = false;

//        AsyncExecutionMode IAsyncEnumerableOperator<T>.ExecutionMode => AsyncExecutionMode.Sequential;

//        public AsyncBehaviorSubject(T first) {
//            this.first = Task.FromResult(first);
//        }

//        public AsyncBehaviorSubject(Task<T> first) {
//            this.first = first;
//        }

//        public AsyncBehaviorSubject(Func<Task<T>> first) {
//            this.first = first();
//        }

//        public Subscriber GetAsyncEnumerator(CancellationToken cancellationToken = default) {
//            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.disposeCancellation.Token);
//            var subscriber = new Subscriber(this, this.first, cancellation.Token);

//            subscribers.Add(subscriber);
//            return subscriber;
//        }

//        IAsyncEnumerator<T> IAsyncEnumerable<T>.GetAsyncEnumerator(CancellationToken cancellationToken) {
//            return this.GetAsyncEnumerator(cancellationToken);
//        }


//        public void AsyncNext(ValueTask<T> item) {
//            if (this.isDisposed) {
//                throw new ObjectDisposedException("Subject has been disposed, cannot call Next()");
//            }

//            foreach (var subscriber in subscribers) {
//                subscriber.Next(item);
//            }
//        }

//        public void AsyncNext(Func<ValueTask<T>> item) => this.AsyncNext(item());

//        public void AsyncNext(Task<T> item) => this.AsyncNext(new ValueTask<T>(item));

//        public void Next(T item) => this.AsyncNext(ValueTask.FromResult(item));

//        public void Dispose() {
//            if (!this.isDisposed) {
//                this.isDisposed = true;
//                this.disposeCancellation.Cancel();
//            }
//        }

//        public class Subscriber : IAsyncEnumerator<T> {
//            private readonly AsyncBehaviorSubject<T> parent;
//            private readonly Channel<ValueTask<T>> channel;
//            private readonly CancellationToken cancellationToken;

//            public T Current { get; private set; } = default!;

//            internal Subscriber(AsyncBehaviorSubject<T> parent, Task<T> first, CancellationToken cancellationToken) {
//                this.parent = parent;
//                this.cancellationToken = cancellationToken;

//                this.channel = Channel.CreateBounded<ValueTask<T>>(new BoundedChannelOptions(1) { 
//                    AllowSynchronousContinuations = true,
//                    FullMode = BoundedChannelFullMode.DropOldest
//                });

//                this.channel.Writer.TryWrite(new ValueTask<T>(first));
//            }

//            internal void Next(ValueTask<T> item) {
//                this.channel.Writer.TryWrite(item);
//            }

//            public ValueTask DisposeAsync() {
//                this.parent.subscribers.Remove(this);

//                return ValueTask.CompletedTask;
//            }

//            public async ValueTask<bool> MoveNextAsync() {
//                // If the subject has been disposed, don't wait on more items
//                if (!this.parent.isDisposed) {
//                    try {
//                        await this.channel.Reader.WaitToReadAsync(this.cancellationToken);
//                    }
//                    catch (OperationCanceledException) {
//                    }
//                }

//                // We should still return items currently in the pipeline though
//                if (!this.channel.Reader.TryRead(out var item)) {
//                    return false;
//                }

//                this.Current = await item;
//                return true;
//            }
//        }
//    }
//}
