using System.Threading.Channels;

namespace AsyncLinq.Operators {
    internal interface IAsyncSelectWhereOperator<E> {
        public IAsyncEnumerable<G> ComposeWith<G>(AsyncSelectWhereFunc<E, G> nextSelector);
    }

    internal delegate ValueTask<SelectWhereResult<E>> AsyncSelectWhereFunc<T, E>(T item);

    internal class AsyncSelectWhereOperator<T, E> : IAsyncOperator<E>, ISelectWhereOperator<E>, IAsyncSelectWhereOperator<E> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly AsyncSelectWhereFunc<T, E> selector;

        public AsyncOperatorParams Params { get; }

        public AsyncSelectWhereOperator(
            IAsyncEnumerable<T> collection,
            AsyncSelectWhereFunc<T, E> selector,
            AsyncOperatorParams pars) {

            this.parent = collection;
            this.selector = selector;
            this.Params = pars;
        }

        public IAsyncEnumerable<G> ComposeWith<G>(SelectWhereFunc<E, G> nextSelector) {
            return new AsyncSelectWhereOperator<T, G>(
                this.parent,
                async item => {
                    var (isValid, value) = await this.selector(item);

                    if (!isValid) {
                        return new(false, default!);
                    }

                    return nextSelector(value);
                },
                this.Params);
        }

        public IAsyncEnumerable<G> ComposeWith<G>(AsyncSelectWhereFunc<E, G> nextSelector) {
            return new AsyncSelectWhereOperator<T, G>(
                this.parent,
                async item => {
                    var (isValid, value) = await this.selector(item);

                    if (!isValid) {
                        return new(false, default!);
                    }

                    return await nextSelector(value);
                },
                this.Params);
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.Params.ExecutionMode == AsyncExecutionMode.Sequential) {
                return this.SequentialHelper(cancellationToken);
            }
            else if (this.Params.IsUnordered) {
                return this.UnorderedHelper(cancellationToken);
            }
            else {
                return this.OrderedHelper(cancellationToken);
            }
        }

        private async IAsyncEnumerator<E> SequentialHelper(CancellationToken cancellationToken) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                var (keep, value) = await this.selector(item);

                if (keep) {
                    yield return value;
                }
            }
        }

        private IAsyncEnumerator<E> UnorderedHelper(CancellationToken cancellationToken) {
            return ParallelHelper.DoUnordered<T, E>(
                this.parent,
                async (x, channel) => {
                    var (keep, value) = await this.selector(x);

                    if (keep) {
                        await channel.Writer.WriteAsync(value, cancellationToken);
                    }
                },
                this.Params.ExecutionMode == AsyncExecutionMode.Parallel,
                cancellationToken);
        }

        private async IAsyncEnumerator<E> OrderedHelper(CancellationToken cancellationToken) {
            // 1. Iterate the enumerable and start the tasks
            // 2. Iterate the tasks and return the results

            var channel = Channel.CreateUnbounded<ValueTask<SelectWhereResult<E>>>(new UnboundedChannelOptions() {
                AllowSynchronousContinuations = true
            });

            // NOTE: This is fire and forget (never awaited) because any exceptions will be propagated 
            // through the channel 
            this.OrderedIterateHelper(channel, cancellationToken);

            while (true) {
                var canRead = await channel.Reader.WaitToReadAsync(cancellationToken);

                if (!canRead) {
                    break;
                }

                if (!channel.Reader.TryRead(out var item)) {
                    break;
                }

                var (isValid, value) = await item;

                if (isValid) {
                    yield return value;
                }
            }
        }

        private async void OrderedIterateHelper(Channel<ValueTask<SelectWhereResult<E>>> channel, CancellationToken token) {
            try {
                var isParallel = this.Params.ExecutionMode == AsyncExecutionMode.Parallel;
                var selector = this.selector;

                if (isParallel) {
                    selector = x => new ValueTask<SelectWhereResult<E>>(Task.Run(() => this.selector(x).AsTask()));
                }

                await foreach (var item in this.parent.WithCancellation(token)) {
                    channel.Writer.TryWrite(selector(item));
                }

                channel.Writer.Complete();
            }
            catch (Exception ex) {
                channel.Writer.Complete(ex);
            }
        }
    }
}
