namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IObservable<TSource> AsObservable<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return new AsyncSubject<TSource>(source);
    }

    private sealed class AsyncSubject<T> : IObservable<T> {
        private readonly IAsyncEnumerable<T> sequence;

        public AsyncSubject(IAsyncEnumerable<T> sequence) {
            this.sequence = sequence;
        }

        public IDisposable Subscribe(IObserver<T> observer) {
            return new Subscription<T>(this.sequence, observer);
        }
    }

    private sealed class Subscription<T> : IDisposable {
        private readonly CancellationTokenSource cancellation = new();
        private bool isDisposed = false;

        public Subscription(IAsyncEnumerable<T> sequence, IObserver<T> target) {
            // Start the iteration on this thread so we create the iterator and attempt
            // a move next, which will add a subscriber if the sequence is a subject
            this.IterateSequence(sequence, target);
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

        private async void IterateSequence(IAsyncEnumerable<T> sequence, IObserver<T> target) {
            try {
                await foreach (var item in sequence.WithCancellation(this.cancellation.Token)) {
                    target.OnNext(item);
                }

                target.OnCompleted();
            }
            catch (Exception ex) {
                target.OnError(ex);
            }                
        }
    }
}