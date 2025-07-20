using System.Threading.Channels;

namespace AsyncAssortments.Operators {
    internal class WrapObservableOperator<T> : IAsyncOperator<T> {
        private readonly IObservable<T> source;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public int MaxConcurrency { get; }

        public WrapObservableOperator(AsyncEnumerableScheduleMode pars, int maxConcurrency, IObservable<T> source) {
            this.ScheduleMode = pars;
            this.MaxConcurrency = maxConcurrency;
            this.source = source;
        }

        public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars, int maxConcurrency) {
            return new WrapObservableOperator<T>(pars, maxConcurrency, this.source);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            Channel<T> channel;

            if (this.MaxConcurrency <= 0) {
                channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions() {
                    AllowSynchronousContinuations = true
                });
            }
            else {
                channel = Channel.CreateBounded<T>(new BoundedChannelOptions(this.MaxConcurrency) {
                    AllowSynchronousContinuations = true,
                    FullMode = BoundedChannelFullMode.DropWrite
                });
            }

            return new ObservableEnumerator(channel, this.source, cancellationToken);
        }

        private class ObservableEnumerator : IAsyncEnumerator<T>, IObserver<T> {
            private readonly Channel<T> channel;
            private readonly IDisposable channelSub;
            private readonly object isCompleteLock = new();
            private bool isComplete = false;

            public ObservableEnumerator(Channel<T> channel, IObservable<T> parent, CancellationToken cancellationToken) {
                this.channel = channel;
                this.channelSub = parent.Subscribe(this);

                // When the cancellation token is triggered, we need to dispose the subscription 
                // so we stop receiving new items.
                cancellationToken.Register(() => this.channelSub.Dispose());
            }

            public T Current { get; private set; } = default!;

            public ValueTask DisposeAsync() {
                this.channelSub.Dispose();

                return default;
            }

            public async ValueTask<bool> MoveNextAsync() {
                var hasMore = await this.channel.Reader.WaitToReadAsync();

                if (!hasMore) {
                    return false;
                }

                if (!this.channel.Reader.TryRead(out var item)) {
                    return false;
                }

                this.Current = item;
                return true;
            }

            public void OnCompleted() {
                // We need to lock to protect against bad implementations of IObservable
                // that complete a sequence twice
                lock (this.isCompleteLock) {
                    if (this.isComplete) {
                        return;
                    }

                    this.isComplete = true;
                    this.channel.Writer.Complete();
                }
            }

            public void OnError(Exception error) {
                // We need to lock to protect against bad implementations of IObservable
                // that complete a sequence twice
                lock (this.isCompleteLock) {
                    if (this.isComplete) {
                        return;
                    }

                    this.isComplete = true;
                    this.channel.Writer.Complete(error);
                }
            }

            public void OnNext(T value) {
                this.channel.Writer.TryWrite(value);
            }
        }
    }
}
