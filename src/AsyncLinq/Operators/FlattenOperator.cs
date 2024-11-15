using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AsyncLinq.Operators {
    internal class FlattenOperator<T> : IAsyncOperator<T> {
        private static readonly UnboundedChannelOptions channelOptions = new UnboundedChannelOptions() {
            AllowSynchronousContinuations = true
        };

        private readonly IAsyncEnumerable<IAsyncEnumerable<T>> parent;

        public AsyncOperatorParams Params { get; }

        public FlattenOperator(AsyncOperatorParams pars, IAsyncEnumerable<IAsyncEnumerable<T>> parent) {
            this.parent = parent;
            Params = pars;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
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

        private async IAsyncEnumerator<T> SequentialHelper(CancellationToken cancellationToken) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                await foreach (var sub in item.WithCancellation(cancellationToken)) {
                    yield return sub;
                }
            }
        }

        private async IAsyncEnumerator<T> OrderedHelper(CancellationToken cancellationToken) {
            var channel = Channel.CreateUnbounded<Channel<T>>(channelOptions);
            var iterateTask = IterateOuter();

            while (true) {
                var canRead = await channel.Reader.WaitToReadAsync(cancellationToken);

                if (!canRead) {
                    break;
                }

                if (!channel.Reader.TryRead(out var subChannel)) {
                    break;
                }

                while (true) {
                    var canReadSub = await subChannel.Reader.WaitToReadAsync(cancellationToken);

                    if (!canReadSub) {
                        break;
                    }

                    if (!subChannel.Reader.TryRead(out var item)) {
                        break;
                    }

                    yield return item;
                }
            }

            // Await the iteration so that exceptions are joined back in
            await iterateTask;

            async Task IterateOuter() {
                try {
                    var tasks = new List<Task>();

                    try {
                        await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                            if (item == EmptyOperator<T>.Instance) {
                                continue;
                            }

                            var subChannel = Channel.CreateUnbounded<T>(channelOptions);
                            var task = IterateInner(item, subChannel);

                            if (!task.IsCompletedSuccessfully) {
                                tasks.Add(task.AsTask());
                            }

                            channel.Writer.TryWrite(subChannel);
                        }
                    }
                    catch (Exception ex) {
                        // Add a task to our list to throw this exception so that
                        // it is included in the aggregate exception, along with
                        // whatever other errors are thrown
                        tasks.Add(new Task(() => throw ex));
                    }

                    await Task.WhenAll(tasks);
                }
                finally {
                    channel.Writer.Complete();
                }
            }

            async ValueTask IterateInner(IAsyncEnumerable<T> seq, Channel<T> channel) {
                try {                   
                    await foreach (var item in seq.WithCancellation(cancellationToken)) {
                        channel.Writer.TryWrite(item);
                    }
                }
                finally {
                    channel.Writer.Complete();
                }                
            }
        }

        private async IAsyncEnumerator<T> UnorderedHelper(CancellationToken cancellationToken) {
            var channel = Channel.CreateUnbounded<T>(channelOptions);
            var iterateTask = IterateOuter();

            while (true) {
                var canRead = await channel.Reader.WaitToReadAsync(cancellationToken);

                if (!canRead) {
                    break;
                }

                if (!channel.Reader.TryRead(out var item)) {
                    break;
                }

                yield return item;
            }

            // Await the iteration so that exceptions are joined back in
            await iterateTask;

            async Task IterateOuter() {
                try {
                    var tasks = new List<Task>();

                    try {
                        await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                            if (item == EmptyOperator<T>.Instance) {
                                continue;
                            }

                            var task = IterateInner(item);

                            if (!task.IsCompletedSuccessfully) {
                                tasks.Add(task.AsTask());
                            }
                        }
                    }
                    catch (Exception ex) {
                        // Add a task to our list to throw this exception so that
                        // it is included in the aggregate exception, along with
                        // whatever other errors are thrown
                        tasks.Add(new Task(() => throw ex));
                    }

                    await Task.WhenAll(tasks);
                }
                finally {
                    channel.Writer.Complete();
                }
            }

            async ValueTask IterateInner(IAsyncEnumerable<T> seq) {               
                await foreach (var item in seq.WithCancellation(cancellationToken)) {
                    channel.Writer.TryWrite(item);
                }
            }
        }
    }
}
