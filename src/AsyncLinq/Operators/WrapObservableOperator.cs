using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AsyncCollections.Linq.Operators {
    internal class WrapObservableOperator<T> : IAsyncOperator<T> {
        private readonly IObservable<T> source;
        private readonly int maxBuffer;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public WrapObservableOperator(AsyncEnumerableScheduleMode pars, IObservable<T> source, int maxBuffer) {
            this.ScheduleMode = pars;
            this.source = source;
            this.maxBuffer = maxBuffer;
        }

        public IAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode pars) {
            return new WrapObservableOperator<T>(pars, this.source, this.maxBuffer);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            Channel<T> channel;

            if (maxBuffer <= 0) {
                channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions() {
                    AllowSynchronousContinuations = true
                });
            }
            else {
                channel = Channel.CreateBounded<T>(new BoundedChannelOptions(maxBuffer) {
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
