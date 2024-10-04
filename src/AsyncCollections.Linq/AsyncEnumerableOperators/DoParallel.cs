using CollectionTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

public static partial class AsyncEnumerableExtensions {
    internal static async IAsyncEnumerable<E> DoParallel<T, E>(this IAsyncEnumerable<T> sequence, Func<T, Channel<E>, ValueTask> action) {
        var result = Channel.CreateUnbounded<E>();        

        var parallelTask = Parallel
            .ForEachAsync(sequence, async (item, _) => {
                await action(item, result);
            })
            .Select(() => result.Writer.Complete());

        try {
            while (true) {
                var canRead = await result.Reader.WaitToReadAsync();

                if (!canRead) {
                    break;
                }

                if (!result.Reader.TryRead(out var item)) {
                    break;
                }

                yield return item;
            }
        }
        finally {
            await parallelTask;
        }
    }

    internal static async IAsyncEnumerable<E> DoConcurrent<T, E>(this IAsyncEnumerable<T> sequence, Func<T, Channel<E>, ValueTask> action) {
        var result = Channel.CreateUnbounded<E>();
        var completionTask = Completion();

        try {
            while (true) {
                var canRead = await result.Reader.WaitToReadAsync();

                if (!canRead) {
                    break;
                }

                if (!result.Reader.TryRead(out var item)) {
                    break;
                }

                yield return item;
            }
        }
        finally {
            await completionTask;
        }

        async ValueTask Completion() {
            var asyncTasks = null as List<ValueTask>;
            var exceptions = null as List<Exception>;

            await foreach (var item in sequence) {
                var task = action(item, result);

                // Don't await or add this to the list if it's already done
                if (task.IsCompletedSuccessfully) {
                    continue;
                }

                if (asyncTasks == null) {
                    asyncTasks = [];
                }

                asyncTasks.Add(task);
            }

            if (asyncTasks != null) {
                foreach (var task in asyncTasks) {
                    try {
                        await task;
                    }
                    catch (AggregateException ex) {
                        if (exceptions == null) {
                            exceptions = [];
                        }

                        exceptions.AddRange(ex.InnerExceptions);
                    }
                    catch (Exception ex) {
                        if (exceptions == null) {
                            exceptions = [];
                        }

                        exceptions.Add(ex);
                    }
                }
            }

            result.Writer.Complete();

            if (exceptions != null) {
                if (exceptions.Count == 1) {
                    throw exceptions[0];
                }
                else {
                    throw new AggregateException(exceptions);
                }
            }
        }
    }
}