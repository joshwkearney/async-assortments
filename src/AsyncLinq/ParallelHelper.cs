using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace AsyncLinq;

internal static class ParallelHelper {
    private static readonly UnboundedChannelOptions parallelChannelOptions = new() {
        AllowSynchronousContinuations = true,
        SingleReader = false,
        SingleWriter = false
    };

    internal static async IAsyncEnumerator<E> DoOrdered<T, E>(
        IAsyncEnumerable<T> sequence,
        Action<T, List<ValueTask<E>>> action,
        CancellationToken cancellationToken) {

        var list = new List<ValueTask<E>>();
        var exceptions = null as List<Exception>;

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            try {
                action(item, list);
            }
            catch (Exception ex) {
                exceptions ??= [];
                exceptions.Add(ex);
            }
        }

        foreach (var task in list) {
            var result = default(E);
            var hasResult = false;

            try {
                result = await task;
                hasResult = true;
            }
            catch (Exception ex) {
                exceptions ??= [];
                exceptions.Add(ex);
            }

            if (hasResult) {
                yield return result!;
            }
        }

        if (exceptions != null && exceptions.Count > 1) {
            throw new AggregateException(exceptions);
        }
        else if (exceptions != null && exceptions.Count == 1) {
            throw exceptions[0];
        }
    }

    internal static async IAsyncEnumerator<E> DoUnordered<T, E>(
        IAsyncEnumerable<T> sequence, 
        Func<T, Channel<E>, ValueTask> action,
        bool isParallel,
        CancellationToken cancellationToken) {

        var result = Channel.CreateUnbounded<E>(parallelChannelOptions);
        var exceptions = Channel.CreateBounded<Exception>(1);

        // NOTE: This is fire and forget (never awaited) because any exceptions will be propagated 
        // through the channel 
        IterateUnorderedHelper(sequence, action, result, isParallel, cancellationToken);

        while (true) {
            var canRead = await result.Reader.WaitToReadAsync(cancellationToken);

            if (!canRead) {
                break;
            }

            if (!result.Reader.TryRead(out var item)) {
                break;
            }

            yield return item;
        }
    }

    private static async void IterateUnorderedHelper<T, E>(
        IAsyncEnumerable<T> sequence,
        Func<T, Channel<E>, ValueTask> action, 
        Channel<E> result,
        bool isParallel,
        CancellationToken cancellationToken) {

        var asyncTasks = null as List<Task>;
        var exceptions = null as List<Exception>;

        try {
            await foreach (var item in sequence.WithCancellation(cancellationToken)) {
                if (isParallel) {
                    var task = Task.Run(() => action(item, result).AsTask(), cancellationToken);

                    asyncTasks ??= [];
                    asyncTasks.Add(task);
                }
                else {
                    var valueTask = action(item, result);

                    if (valueTask.IsCompletedSuccessfully) {
                        continue;
                    }

                    var task = valueTask.AsTask();

                    if (task.Exception != null) {
                        exceptions ??= [];
                        exceptions.Add(task.Exception);
                    }
                    else {
                        asyncTasks ??= [];
                        asyncTasks.Add(task);
                    }
                }
            }
        }
        catch (Exception ex) {
            exceptions ??= [];
            exceptions.Add(ex);
        }

        if (asyncTasks != null) {
            foreach (var task in asyncTasks) {
                if (task.Exception != null) {
                    exceptions ??= [];
                    exceptions.Add(task.Exception);

                    continue;
                }

                try {
                    await task;
                }
                catch (Exception ex) {
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }
        }

        if (exceptions == null) {
            result.Writer.Complete();
        }
        else if (exceptions.Count == 1) {
            result.Writer.Complete(exceptions[0]);
        }
        else {
            result.Writer.Complete(new AggregateException(exceptions));
        }
    }
}