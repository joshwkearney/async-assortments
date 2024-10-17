namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IObservable<T> AsObservable<T>(this IAsyncEnumerable<T> sequence) {
        return new AsyncSubject<T>(sequence);
    }

    private sealed class AsyncSubject<T> : IObservable<T> {
        private readonly List<IObserver<T>> subscribers = [];
        private readonly IAsyncEnumerable<T> sequence;

        public AsyncSubject(IAsyncEnumerable<T> sequence) {
            this.sequence = sequence;
        }

        public IDisposable Subscribe(IObserver<T> observer) {
            this.subscribers.Add(observer);

            return new Subscription<T>(this.sequence, observer);
        }
    }

    private class Subscription<T> : IDisposable {
        private readonly IAsyncEnumerable<T> sequence;
        private readonly IObserver<T> target;
        private readonly CancellationTokenSource cancellation = new();
        private bool isDisposed = false;

        public Subscription(IAsyncEnumerable<T> sequence, IObserver<T> target) {
            this.target = target;
            this.sequence = sequence;

            // Start the iteration on this thread so we create the iterator and attempt
            // a move next, which will add a subscriber if the sequence is a subject
            this.IterateSequence();
        }

        public void Dispose() {
            if (this.isDisposed) {
                return;
            }

            try {
                this.cancellation.Cancel();
            }
            finally {
                this.isDisposed = true;
            }
        }

        private async void IterateSequence() {
            try {
                await foreach (var item in sequence.WithCancellation(this.cancellation.Token)) {
                    this.target.OnNext(item);
                }

                this.target.OnCompleted();
            }
            catch (Exception ex) {
                this.target.OnError(ex);
            }                
        }
    }
}