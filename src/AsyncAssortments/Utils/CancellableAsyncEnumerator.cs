namespace AsyncAssortments {
    internal class CancellableAsyncEnumerator<T> : IAsyncEnumerator<T> {
        private readonly IAsyncEnumerator<T> enumerator;
        private readonly CancellationTokenSource cancellationSource;
        private bool isDisposed = false;

        public T Current => this.enumerator.Current;

        public CancellableAsyncEnumerator(CancellationTokenSource cancellationSource, IAsyncEnumerator<T> enumerator) {
            this.enumerator = enumerator;
            this.cancellationSource = cancellationSource;
        }
        
        public ValueTask DisposeAsync() {
            if (this.isDisposed) {
                return new ValueTask();
            }

            this.cancellationSource.Cancel();
            this.cancellationSource.Dispose();
            this.isDisposed = true;

            return new ValueTask();
        }

        public ValueTask<bool> MoveNextAsync() => this.enumerator.MoveNextAsync();
    }
}