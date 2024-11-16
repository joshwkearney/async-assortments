using System;
using System.Threading.Channels;
using System.Transactions;

namespace AsyncLinq.Operators {
    internal interface ISelectWhereTaskOperator<E> : IAsyncOperator<E> {
        public IAsyncEnumerable<G> SelectWhereTask<G>(AsyncSelectWhereFunc<E, G> nextSelector);
    }

    internal delegate ValueTask<SelectWhereResult<E>> AsyncSelectWhereFunc<T, E>(T item);

    internal class SelectWhereTaskOperator<T, E> : IAsyncOperator<E>, ISelectWhereOperator<E>, 
        ISelectWhereTaskOperator<E> {

        private static readonly UnboundedChannelOptions channelOptions = new UnboundedChannelOptions() {
            AllowSynchronousContinuations = true
        };

        private readonly IAsyncEnumerable<T> parent;
        private readonly AsyncSelectWhereFunc<T, E> selector;

        public AsyncOperatorParams Params { get; }

        public SelectWhereTaskOperator(
            AsyncOperatorParams pars,
            IAsyncEnumerable<T> collection,
            AsyncSelectWhereFunc<T, E> selector) {

            this.parent = collection;
            this.selector = selector;
            this.Params = pars;
        }
        
        public IAsyncOperator<E> WithParams(AsyncOperatorParams pars) {
            return new SelectWhereTaskOperator<T, E>(pars, parent, selector);
        }

        public IAsyncEnumerable<G> SelectWhere<G>(SelectWhereFunc<E, G> nextSelector) {
            return new SelectWhereTaskOperator<T, G>(this.Params, this.parent, newSelector);

            async ValueTask<SelectWhereResult<G>> newSelector(T item) {
                var (isValid, value) = await this.selector(item);

                if (!isValid) {
                    return new(false, default!);
                }

                return nextSelector(value);
            }
        }

        public IAsyncEnumerable<G> SelectWhereTask<G>(AsyncSelectWhereFunc<E, G> nextSelector) {
            return new SelectWhereTaskOperator<T, G>(this.Params, this.parent, newSelector);

            async ValueTask<SelectWhereResult<G>> newSelector(T item) {
                var (isValid, value) = await this.selector(item);

                if (!isValid) {
                    return new(false, default!);
                }

                return await nextSelector(value);
            }
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

        private async IAsyncEnumerator<E> UnorderedHelper(CancellationToken cancellationToken) {
            var isParallel = this.Params.ExecutionMode == AsyncExecutionMode.Parallel;
            var selector = this.selector;

            // Run the tasks on the thread pool if we're supposed to be doing this in parallel
            if (isParallel) {
                selector = x => new ValueTask<SelectWhereResult<E>>(Task.Run(() => this.selector(x).AsTask()));
            }

            var channel = Channel.CreateUnbounded<E>(channelOptions);

            // This is fire and forget (never awaited) because any exceptions will be propagated 
            // through the channel 
            Iterate();

            while (true) {
                // This may throw an exception, but we don't need to handle it because that is done
                // in the iterate method
                var canRead = await channel.Reader.WaitToReadAsync(cancellationToken);

                if (!canRead) {
                    break;
                }

                if (!channel.Reader.TryRead(out var item)) {
                    break;
                }

                yield return item;
            }

            async ValueTask AddToChannel(T item) {
                var (isValid, value) = await selector(item);

                if (isValid) {
                    channel.Writer.TryWrite(value);
                }
            }

            async void Iterate() {
                var asyncTasks = new List<Task>();
                var errors = new ErrorCollection();

                try {
                    await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                        var valueTask = AddToChannel(item);

                        if (valueTask.IsCompletedSuccessfully) {
                            continue;
                        }

                        asyncTasks.Add(valueTask.AsTask());
                    }
                }
                catch (Exception ex) {
                    // Whoops, our async enumerable blew up. Catch it so we can handle errors from the
                    // async tasks that are already running
                    errors.Add(ex);
                }

                foreach (var task in asyncTasks) {
                    if (task.Exception != null) {
                        errors.Add(task.Exception);

                        continue;
                    }

                    try {
                        await task;
                    }
                    catch (Exception ex) {
                        errors.Add(ex);
                    }
                }

                var finalError = errors.ToException();

                if (finalError != null) {
                    channel.Writer.Complete(finalError);
                }
                else {
                    channel.Writer.Complete();
                }
            }
        }        

        private async IAsyncEnumerator<E> OrderedHelper(CancellationToken cancellationToken) {
            var isParallel = this.Params.ExecutionMode == AsyncExecutionMode.Parallel;
            var selector = this.selector;

            // Run the tasks on the thread pool if we're supposed to be doing this in parallel
            if (isParallel) {
                selector = x => new ValueTask<SelectWhereResult<E>>(Task.Run(() => this.selector(x).AsTask()));
            }

            var errors = new ErrorCollection();
            var channel = Channel.CreateUnbounded<ValueTask<SelectWhereResult<E>>>(channelOptions);            
            
            // Fire and forget because exceptions will be handled through the channel
            Iterate();

            while (true) {
                var isValid = false;
                var value = default(E);

                try {
                    var canRead = await channel.Reader.WaitToReadAsync(cancellationToken);

                    if (!canRead) {
                        break;
                    }
                }
                catch (Exception ex) {
                    // If the whole channel blows up, we're done
                    errors.Add(ex);
                    break;
                }

                try {                 
                    if (!channel.Reader.TryRead(out var item)) {
                        break;
                    }

                    var pair = await item;

                    isValid = pair.IsValid;
                    value = pair.Value;
                }
                catch (Exception ex) {
                    // If just one item blows up, keep fetching the next ones
                    errors.Add(ex);
                    isValid = false;
                }

                if (isValid) {
                    yield return value!;
                }
            }

            var finalError = errors.ToException();

            if (finalError != null) {
                throw finalError;
            }

            // This will go through the async enumerable and start all the tasks, which are then
            // put in the channel in order. They will be awaited in the reading loop. Exceptions
            // thrown by the selector are also handled in the reading loop
            async void Iterate() {
                try {
                    await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                        channel.Writer.TryWrite(selector(item));
                    }
                }
                catch (Exception ex) {
                    channel.Writer.Complete(ex);
                }
                finally {
                    channel.Writer.Complete();
                }
            }
        }        
    }
}
