using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AsyncLinq.Operators {    
    internal class FlattenOperator<T> : IScheduledAsyncOperator<T>, IConcatOperator<T>, IConcatEnumerablesOperator<T> {
        private static readonly UnboundedChannelOptions channelOptions = new UnboundedChannelOptions() {
            AllowSynchronousContinuations = true
        };

        private readonly IAsyncEnumerable<IAsyncEnumerable<T>> parent;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public FlattenOperator(AsyncEnumerableScheduleMode pars, IAsyncEnumerable<IAsyncEnumerable<T>> parent) {
            this.parent = parent;
            ScheduleMode = pars;
        }
        
        public IScheduledAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode pars) {
            return new FlattenOperator<T>(pars, parent);
        }
        
        public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence) {
            if (this.parent is EnumerableOperator<IAsyncEnumerable<T>> op) {
                var newItems = op.Items.Append(sequence);
                var newParent = new EnumerableOperator<IAsyncEnumerable<T>>(op.ScheduleMode, newItems);
                
                return new FlattenOperator<T>(this.ScheduleMode, newParent);
            }
            else {
                return new FlattenOperator<T>(this.ScheduleMode, new[] { this, sequence }.ToAsyncEnumerable());
            }
        }

        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
            if (this.parent is EnumerableOperator<IAsyncEnumerable<T>> op) {
                var newItems = op.Items.Prepend(before.ToAsyncEnumerable()).Append(after.ToAsyncEnumerable());
                var newParent = new EnumerableOperator<IAsyncEnumerable<T>>(op.ScheduleMode, newItems);
                
                return new FlattenOperator<T>(this.ScheduleMode, newParent);
            }
            else {
                return new ConcatEnumerablesOperator<T>(this.ScheduleMode, this, before, after);
            }
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ScheduleMode == AsyncEnumerableScheduleMode.Sequential) {
                return this.SequentialHelper(cancellationToken);
            }
            else if (this.ScheduleMode.IsUnordered()) {
                return this.UnorderedHelper(cancellationToken);
            }
            else {
                return this.OrderedHelper(cancellationToken);
            }
        }        

        private async IAsyncEnumerator<T> SequentialHelper(CancellationToken cancellationToken) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                await foreach (var sub in item.WithCancellation(cancellationToken)) {
                    yield return sub;
                }
            }
        }

        private async IAsyncEnumerator<T> OrderedHelper(CancellationToken parentToken) {
            using var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
            var channel = Channel.CreateUnbounded<Channel<T>>(channelOptions);
            var errors = new ErrorCollection();
            
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

                        var subChannel = Channel.CreateUnbounded<T>(channelOptions);
                        channel.Writer.TryWrite(subChannel);

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
                        subChannel.Writer.TryWrite(item);
                    }

                    subChannel.Writer.Complete();
                }
                catch (Exception ex) {
                    subChannel.Writer.Complete(ex);
                }
            }
        }

        private async IAsyncEnumerator<T> UnorderedHelper(CancellationToken parentToken) {
            using var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
            var channel = Channel.CreateUnbounded<T>(channelOptions);
            
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

                        ValueTask task;

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
                await foreach (var item in seq.WithCancellation(cancelSource.Token)) {
                    channel.Writer.TryWrite(item);
                }
            }
        }
    }
}
