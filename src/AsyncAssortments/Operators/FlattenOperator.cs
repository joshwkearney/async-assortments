using System.Threading.Channels;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace AsyncAssortments.Operators {    
    internal class FlattenOperator<T> : IAsyncOperator<T>, IConcatOperator<T>, IConcatEnumerablesOperator<T> {
        private readonly IAsyncEnumerable<IAsyncEnumerable<T>> parent;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public int MaxConcurrency { get; }

        public FlattenOperator(AsyncEnumerableScheduleMode pars, int maxConcurrency, IAsyncEnumerable<IAsyncEnumerable<T>> parent) {
            this.parent = parent;
            this.ScheduleMode = pars;
            this.MaxConcurrency = maxConcurrency;
        }
        
        public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars, int maxConcurrency) {
            return new FlattenOperator<T>(pars, maxConcurrency, parent);
        }
        
        public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence) {
            if (this.parent is WrapEnumerableOperator<IAsyncEnumerable<T>> op) {
                var newItems = op.Items.Append(sequence);
                var newParent = new WrapEnumerableOperator<IAsyncEnumerable<T>>(op.ScheduleMode, op.MaxConcurrency, newItems);
                
                return new FlattenOperator<T>(this.ScheduleMode, this.MaxConcurrency, newParent);
            }
            else {
                return new FlattenOperator<T>(this.ScheduleMode, this.MaxConcurrency, new[] { this, sequence }.ToAsyncEnumerable());
            }
        }

        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
            if (this.parent is WrapEnumerableOperator<IAsyncEnumerable<T>> op) {
                var newItems = op.Items.Prepend(before.ToAsyncEnumerable()).Append(after.ToAsyncEnumerable());
                var newParent = new WrapEnumerableOperator<IAsyncEnumerable<T>>(op.ScheduleMode, op.MaxConcurrency,newItems);
                
                return new FlattenOperator<T>(this.ScheduleMode, this.MaxConcurrency,newParent);
            }
            else {
                return new ConcatEnumerablesOperator<T>(this.ScheduleMode, this.MaxConcurrency, this, before, after);
            }
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ScheduleMode == AsyncEnumerableScheduleMode.Sequential || this.MaxConcurrency == 1) {
                return this.SequentialHelper(cancellationToken);
            }
            
            var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            
            if (this.ScheduleMode.IsUnordered()) {
                return new CancellableAsyncEnumerator<T>(cancelSource, this.UnorderedHelper(cancelSource));
            }
            else {
                return new CancellableAsyncEnumerator<T>(cancelSource, this.OrderedHelper(cancelSource));
            }
        }        

        private async IAsyncEnumerator<T> SequentialHelper(CancellationToken cancellationToken) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                await foreach (var sub in item.WithCancellation(cancellationToken)) {
                    yield return sub;
                }
            }
        }

        private async IAsyncEnumerator<T> OrderedHelper(CancellationTokenSource cancelSource) {
            var errors = new ErrorCollection();
            Channel<Channel<T>> channel;

            if (this.MaxConcurrency <= 0) {
                channel = Channel.CreateUnbounded<Channel<T>>(new() {
                    AllowSynchronousContinuations = true                    
                });
            }
            else {
                channel = Channel.CreateBounded<Channel<T>>(new BoundedChannelOptions(this.MaxConcurrency) {
                    AllowSynchronousContinuations = true,
                    FullMode = BoundedChannelFullMode.Wait
                });
            }

            IterateOuter();

            while (true) {
                try {
                    // Don't pass a cancellation token here
                    var canRead = await channel.Reader.WaitToReadAsync();

                    if (!canRead) {
                        break;
                    }
                }
                catch (Exception ex) {
                    errors.Add(ex);
                    cancelSource.Cancel();
                    break;
                }

                if (!channel.Reader.TryRead(out var subChannel)) {
                    break;
                }

                while (true) {
                    try {
                        // Also don't pass a cancellation source here
                        var canReadSub = await subChannel.Reader.WaitToReadAsync();

                        if (!canReadSub) {
                            break;
                        }
                    }
                    catch (Exception ex) {
                        errors.Add(ex);
                        cancelSource.Cancel();
                        break;
                    }

                    if (!subChannel.Reader.TryRead(out var item)) {
                        break;
                    }

                    yield return item;
                }
            }
            
            var finalError = errors.ToException();

            if (finalError != null) {
                throw finalError;
            }

            async void IterateOuter() {
                try {
                    await foreach (var item in this.parent.WithCancellation(cancelSource.Token)) {
                        if (item == EmptyOperator<T>.Instance) {
                            continue;
                        }

                        var subChannel = Channel.CreateUnbounded<T>(new() {
                            AllowSynchronousContinuations = true
                        });

                        // If we're operating with a max concurrency, our channel will have a capped 
                        // capacity and this will block, preventing us from continuting to spawn 
                        // more concurrent tasks
                        await channel.Writer.WriteAsync(subChannel);

                        if (this.ScheduleMode.IsParallel()) {
                            Task.Run(() => IterateInner(item, subChannel));
                        }
                        else {
                            IterateInner(item, subChannel);
                        }
                    }
                    
                    channel.Writer.Complete();
                }
                catch (Exception ex) {
                    channel.Writer.Complete(ex);
                }
            }

            async void IterateInner(IAsyncEnumerable<T> seq, Channel<T> subChannel) {
                try {
                    await foreach (var item in seq.WithCancellation(cancelSource.Token)) {
                        await subChannel.Writer.WriteAsync(item);
                    }

                    subChannel.Writer.Complete();
                }
                catch (Exception ex) {
                    subChannel.Writer.Complete(ex);
                }
            }
        }

        private async IAsyncEnumerator<T> UnorderedHelper(CancellationTokenSource cancelSource) {
            // If we have a max concurrency, we're going to use a semaphore to gate how many concurrent tasks
            // we can spawn at once. This is because max concurrency is about how many tasks spawn, not how
            // many items are in the channel. We can't use a bounded channel for this control here
            SemaphoreSlim semaphore = null!;
            if (this.MaxConcurrency > 0) {
                semaphore = new SemaphoreSlim(this.MaxConcurrency, this.MaxConcurrency);
            }

            var channel = Channel.CreateUnbounded<T>(new() {
                AllowSynchronousContinuations = true
            });

            // This is async void because exceptions will be handled through the channel
            IterateOuter();

            while (true) {
                // Don't pass a cancellation token here, we want the channel to finish on its own
                var canRead = await channel.Reader.WaitToReadAsync();

                if (!canRead) {
                    break;
                }

                if (!channel.Reader.TryRead(out var item)) {
                    break;
                }

                yield return item;
            }

            async void IterateOuter() {
                var tasks = new List<Task>();
                var errors = new ErrorCollection();
                
                try {
                    await foreach (var item in this.parent.WithCancellation(cancelSource.Token)) {
                        if (item == EmptyOperator<T>.Instance) {
                            continue;
                        }

                        // If we have a max concurrency, aquire a semaphore lease first
                        if (this.MaxConcurrency > 0) {
                            await semaphore.WaitAsync(cancelSource.Token);
                        }

                        ValueTask task;

                        // Run our concurrent task. This will release the semaphore when it's done
                        if (this.ScheduleMode.IsParallel()) {
                            task = new ValueTask(Task.Run(() => IterateInner(item).AsTask()));
                        }
                        else {
                            task = IterateInner(item);
                        }

                        if (!task.IsCompletedSuccessfully) {
                            tasks.Add(task.AsTask());
                        }
                    }
                }
                catch (Exception ex) {
                    // Whoops, our async enumerable blew up. Catch it so we can handle errors from the
                    // async tasks that are already running
                    errors.Add(ex);
                    cancelSource.Cancel();
                }

                foreach (var task in tasks) {
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

                if (finalError == null) {
                    channel.Writer.Complete();
                }
                else {
                    channel.Writer.Complete(finalError);
                }
            }

            async ValueTask IterateInner(IAsyncEnumerable<T> seq) {
                try {
                    await foreach (var item in seq.WithCancellation(cancelSource.Token)) {
                        await channel.Writer.WriteAsync(item);
                    }
                }
                finally {
                    if (this.MaxConcurrency > 0) {
                        semaphore.Release();
                    }
                }
            }
        }
    }
}
