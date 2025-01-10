using System.Threading.Channels;

namespace AsyncAssortments.Operators {
    internal delegate ValueTask<SelectWhereResult<E>> AsyncSelectWhereFunc<T, E>(T item, CancellationToken cancellationToken);

    internal class SelectWhereTaskOperator<T, E> : IAsyncOperator<E>, ISelectOperator<E>, 
        IWhereOperator<E>, IAsyncSelectOperator<E>, IAsyncWhereOperator<E> {

        private static readonly UnboundedChannelOptions channelOptions = new UnboundedChannelOptions() {
            AllowSynchronousContinuations = true
        };

        private readonly IAsyncEnumerable<T> parent;
        private readonly AsyncSelectWhereFunc<T, E> selector;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public SelectWhereTaskOperator(
            AsyncEnumerableScheduleMode pars,
            IAsyncEnumerable<T> collection,
            AsyncSelectWhereFunc<T, E> selector) {

            this.parent = collection;
            this.selector = selector;
            this.ScheduleMode = pars;
        }
        
        public IAsyncOperator<E> WithScheduleMode(AsyncEnumerableScheduleMode pars) {
            return new SelectWhereTaskOperator<T, E>(pars, parent, selector);
        }

        public IAsyncEnumerable<G> Select<G>(Func<E, G> selector) {
            return new SelectWhereTaskOperator<T, G>(this.ScheduleMode, this.parent, newSelector);

            async ValueTask<SelectWhereResult<G>> newSelector(T item, CancellationToken token) {
                var (isValid, value) = await this.selector(item, token);

                if (!isValid) {
                    return new(false, default!);
                }

                return new SelectWhereResult<G>(true, selector(value));
            }
        }

        public IAsyncEnumerable<E> Where(Func<E, bool> predicate) {
            return new SelectWhereTaskOperator<T, E>(this.ScheduleMode, this.parent, newSelector);

            async ValueTask<SelectWhereResult<E>> newSelector(T item, CancellationToken token) {
                var (isValid, value) = await this.selector(item, token);

                if (!isValid) {
                    return new(false, default!);
                }

                if (!predicate(value)) {
                    return new(false, default!);
                }

                return new SelectWhereResult<E>(true, value);
            }
        }

        public IAsyncEnumerable<G> AsyncSelect<G>(Func<E, CancellationToken, ValueTask<G>> nextSelector) {
            return new SelectWhereTaskOperator<T, G>(this.ScheduleMode, this.parent, newSelector);

            async ValueTask<SelectWhereResult<G>> newSelector(T item, CancellationToken token) {
                var (isValid, value) = await this.selector(item, token);

                if (!isValid) {
                    return new SelectWhereResult<G>(false, default!);
                }

                return new SelectWhereResult<G>(true, await nextSelector(value, token));
            }
        }

        public IAsyncEnumerable<E> AsyncWhere(Func<E, CancellationToken, ValueTask<bool>> predicate) {
            return new SelectWhereTaskOperator<T, E>(this.ScheduleMode, this.parent, newSelector);

            async ValueTask<SelectWhereResult<E>> newSelector(T item, CancellationToken token) {
                var (isValid, value) = await this.selector(item, token);

                if (!isValid) {
                    return new SelectWhereResult<E>(false, default!);
                }

                return new SelectWhereResult<E>(await predicate(value, token), value);
            }
        }

        public IAsyncEnumerator<E> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ScheduleMode == AsyncEnumerableScheduleMode.Sequential) {
                return this.SequentialHelper(cancellationToken);
            }
            
            var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            if (this.ScheduleMode.IsUnordered()) {
                return new CancellableAsyncEnumerator<E>(cancelSource, this.UnorderedHelper(cancelSource));
            }
            else {
                return new CancellableAsyncEnumerator<E>(cancelSource, this.OrderedHelper(cancelSource));
            }
        }

        private async IAsyncEnumerator<E> SequentialHelper(CancellationToken cancellationToken) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                var (keep, value) = await this.selector(item, cancellationToken);

                if (keep) {
                    yield return value;
                }
            }
        }

        private async IAsyncEnumerator<E> UnorderedHelper(CancellationTokenSource cancelSource) {
            var selector = this.selector;

            // Run the tasks on the thread pool if we're supposed to be doing this in parallel
            if (this.ScheduleMode.IsParallel()) {
                selector = (x, t) => new ValueTask<SelectWhereResult<E>>(Task.Run(() => this.selector(x, t).AsTask(), t));
            }

            var channel = Channel.CreateUnbounded<E>(channelOptions);

            // This is fire and forget (never awaited) because any exceptions will be propagated 
            // through the channel 
            Iterate();

            while (true) {
                // This may throw an exception, but we don't need to handle it because that is done
                // in the iterate method. Don't pass the token here, we want to receive the exception
                // from the other side
                var canRead = await channel.Reader.WaitToReadAsync();

                if (!canRead) {
                    break;
                }

                if (!channel.Reader.TryRead(out var item)) {
                    break;
                }

                yield return item;
            }

            async ValueTask AddToChannel(T item) {
                var (isValid, value) = await selector(item, cancelSource.Token);

                if (isValid) {
                    channel.Writer.TryWrite(value);
                }
            }

            async void Iterate() {
                var asyncTasks = new List<Task>();
                var errors = new ErrorCollection();

                try {
                    await foreach (var item in this.parent.WithCancellation(cancelSource.Token)) {
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
                    cancelSource.Cancel();
                }

                foreach (var task in asyncTasks) {
                    if (task.Exception != null) {
                        errors.Add(task.Exception);
                        cancelSource.Cancel();
                        
                        continue;
                    }

                    try {
                        await task;
                    }
                    catch (Exception ex) {
                        errors.Add(ex);
                        cancelSource.Cancel();
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

        private async IAsyncEnumerator<E> OrderedHelper(CancellationTokenSource cancelSource) {
            var selector = this.selector;

            // Run the tasks on the thread pool if we're supposed to be doing this in parallel
            if (this.ScheduleMode.IsParallel()) {
                selector = (x, t) => new ValueTask<SelectWhereResult<E>>(Task.Run(() => this.selector(x, t).AsTask(), t));
            }

            var errors = new ErrorCollection();
            var channel = Channel.CreateUnbounded<ValueTask<SelectWhereResult<E>>>(channelOptions);            
            
            // Fire and forget because exceptions will be handled through the channel
            Iterate();

            while (true) {
                var isValid = false;
                var value = default(E);

                try {
                    // Don't pass a cancellation token here, we want the channel to finish, either normally
                    // or with an exception from the other side
                    var canRead = await channel.Reader.WaitToReadAsync();

                    if (!canRead) {
                        break;
                    }
                }
                catch (Exception ex) {
                    // If the whole channel blows up, we're done
                    errors.Add(ex);
                    cancelSource.Cancel();
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
                    // If just one item blows up, keep fetching the next ones, but cancel any ongoing work so we
                    // can wrap up
                    errors.Add(ex);
                    cancelSource.Cancel();
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
                    await foreach (var item in this.parent.WithCancellation(cancelSource.Token)) {
                        channel.Writer.TryWrite(selector(item, cancelSource.Token));
                    }
                    
                    channel.Writer.Complete();
                }
                catch (Exception ex) {
                    channel.Writer.Complete(ex);
                }
            }
        }
    }
}
