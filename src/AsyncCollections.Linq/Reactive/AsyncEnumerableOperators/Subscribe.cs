//using AsyncCollections.Reactive.Subjects;

//namespace AsyncCollections.Reactive {
//    public interface ISubscriptionHandle : IAsyncDisposable {
//        public ValueTask WaitAsync();
//    }

//    public static partial class AsyncEnumerableReactiveExtensions {
//        public static ISubscriptionHandle Subscribe<T>(this IAsyncEnumerable<T> sequence, Action<T> onNext) {
//            return new Subscription<T>(sequence, x => { onNext(x); return ValueTask.CompletedTask; });
//        }

//        public static ISubscriptionHandle Subscribe<T>(this IAsyncEnumerable<T> sequence, IAsyncSubject<T> receiver) {
//            return sequence.Subscribe(x => receiver.Next(x));
//        }

//        public static ISubscriptionHandle AsyncSubscribe<T>(this IAsyncEnumerable<T> sequence, Func<T, ValueTask> onNext) {
//            return new Subscription<T>(sequence, onNext);
//        }

//        private class Subscription<T> : ISubscriptionHandle {
//            private readonly IAsyncEnumerable<T> sequence;
//            private readonly Func<T, ValueTask> onNext;
//            private readonly CancellationTokenSource cancellation = new();
//            private readonly ValueTask continueIteration;
//            private bool isDisposed = false;

//            public Subscription(IAsyncEnumerable<T> sequence, Func<T, ValueTask> onNext) {
//                this.onNext = onNext;
//                this.sequence = sequence;

//                // Start the iteration on this thread so we create the iterator and attempt
//                // a move next, which will add a subscriber if the sequence is a subject
//                this.continueIteration = this.IterateSequence();
//            }

//            public async ValueTask WaitAsync() {
//                await this.continueIteration;
//            }

//            public async ValueTask DisposeAsync() {
//                if (this.isDisposed) {
//                    return;
//                }

//                try {
//                    this.cancellation.Cancel();
//                    await this.continueIteration;
//                }
//                catch (OperationCanceledException) {
//                    // This is expected and shouldn't throw when we dispose it. Other exceptions
//                    // should get throw however
//                }
//                finally {
//                    this.isDisposed = true;
//                }
//            }

//            private async ValueTask IterateSequence() {
//                await foreach (var item in sequence.WithCancellation(this.cancellation.Token)) {
//                    await onNext(item);
//                }
//            }
//        }
//    }
//}
