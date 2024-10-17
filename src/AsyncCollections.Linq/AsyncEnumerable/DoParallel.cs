using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    private static readonly UnboundedChannelOptions parallelChannelOptions = new() {
        AllowSynchronousContinuations = true,
        SingleReader = false,
        SingleWriter = false
    };

    internal static IAsyncEnumerable<E> DoParallel<T, E>(
        this IAsyncEnumerable<T> sequence, 
        Func<T, Channel<E>, ValueTask> action) {

        return DoConcurrentHelper(sequence, action, true);
    }

    internal static IAsyncEnumerable<E> DoConcurrent<T, E>(
        this IAsyncEnumerable<T> sequence,
        Func<T, Channel<E>, ValueTask> action) {

        return DoConcurrentHelper(sequence, action, false);
    }

    private static async IAsyncEnumerable<E> DoConcurrentHelper<T, E>(
        this IAsyncEnumerable<T> sequence,
        Func<T, Channel<E>, ValueTask> action,
        bool isParallel,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        var result = Channel.CreateUnbounded<E>(parallelChannelOptions);
        var exceptions = Channel.CreateBounded<Exception>(1);

        // NOTE: This is fire and forget (never awaited) because any exceptions will be propagated 
        // through the channel 
        IterateConcurrently(sequence, action, result, isParallel, cancellationToken);

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

    private static async void IterateConcurrently<T, E>(
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
                    var valuetask = action(item, result);

                    if (valuetask.IsCompletedSuccessfully) {
                        continue;
                    }

                    var task = valuetask.AsTask();

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
        else { 
            if (exceptions.Count == 1) {
                result.Writer.Complete(exceptions[0]);
            }
            else {
                result.Writer.Complete(new AggregateException(exceptions));
            }
        }
    }
}