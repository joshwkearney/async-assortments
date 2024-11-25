using System.Threading.Channels;
using AsyncCollections.Linq.Operators;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerableExtensions {
    /// <summary>
    ///     Converts an <see cref="IObservable{T}" /> into an <see cref="IAsyncEnumerable{T}" />
    ///     by buffering the observable's elements.
    /// </summary>
    /// <param name="maxBuffer">
    ///     The maximum number of items to buffer before new items are dropped.
    /// </param>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> whose enumerators subscribe to the observable
    ///     and yield any items that are pushed to the observable's subscribers.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <remarks>
    ///     This method uses a channel to buffer elements pushed by the observable so that the
    ///     next call to <see cref="IAsyncEnumerator{T}.MoveNextAsync" /> can return it. You 
    ///     may set a maximum number of items to store in this channel, after which new items
    ///     are dropped and not returned by the resulting <see cref="IAsyncEnumerable{T}" />.
    /// </remarks>
    public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(
        this IObservable<TSource> source, 
        int maxBuffer = -1) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return new ObservableWrapper<TSource>(AsyncEnumerableScheduleMode.ConcurrentOrdered, source, maxBuffer);
    }

    private class ObservableWrapper<T> : IAsyncOperator<T> {
        private readonly IObservable<T> source;
        private readonly int maxBuffer;
        
        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public ObservableWrapper(AsyncEnumerableScheduleMode pars, IObservable<T> source, int maxBuffer) {
            this.ScheduleMode = pars;
            this.source = source;
            this.maxBuffer = maxBuffer;
        }
        
        public IAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode pars) {
            return new ObservableWrapper<T>(pars, this.source, this.maxBuffer);
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

            var channelObserver = new ChannelObserver<T>(channel);
            var sub = this.source.Subscribe(channelObserver);

            return new Enumerator(channel, sub, cancellationToken);
        }

        private class Enumerator : IAsyncEnumerator<T> {
            private readonly Channel<T> channel;
            private readonly IDisposable channelSub;
            private readonly CancellationToken cancellationToken;

            public Enumerator(Channel<T> channel, IDisposable channelSub, CancellationToken cancellationToken) {
                this.channel = channel;
                this.channelSub = channelSub;
                this.cancellationToken = cancellationToken;
            }

            public T Current { get; private set; } = default!;

            public ValueTask DisposeAsync() {
                this.channelSub.Dispose();

                return default;
            }

            public async ValueTask<bool> MoveNextAsync() {
                var hasMore = await this.channel.Reader.WaitToReadAsync(this.cancellationToken);

                if (!hasMore) {
                    return false;
                }

                if (!this.channel.Reader.TryRead(out var item)) {
                    return false;
                }

                this.Current = item;
                return true;
            }
        }
    }

    private class ChannelObserver<T> : IObserver<T> {
        private readonly Channel<T> channel;

        public ChannelObserver(Channel<T> channel) {
            this.channel = channel;
        }

        public void OnCompleted() {
            this.channel.Writer.Complete();
        }

        public void OnError(Exception error) {
            this.channel.Writer.Complete();
        }

        public void OnNext(T value) {
            this.channel.Writer.TryWrite(value);
        }
    }    
}