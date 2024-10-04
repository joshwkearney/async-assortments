using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> sequence, IAsyncEnumerable<T> other) {
        if (sequence is IAsyncEnumerableOperator<T> col1 && other is IAsyncEnumerableOperator<T> col2) {
            return new ConcatOperator<T>(col1, col2);
        }

        return ConcatHelper(sequence, other);
    }

    public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> sequence, IEnumerable<T> other) {
        return sequence.Concat(other.AsAsyncEnumerable());
    }

    private static async IAsyncEnumerable<T> ConcatHelper<T>(this IAsyncEnumerable<T> sequence, IAsyncEnumerable<T> other) {
        await foreach (var item in sequence) {
            yield return item;
        }

        await foreach (var item in other) {
            yield return item;
        }
    }

    private static async IAsyncEnumerable<T> ConcurrentConcatHelper<T>(this IAsyncEnumerable<T> sequence, IAsyncEnumerable<T> other) {
        var channel = Channel.CreateUnbounded<T>();
        
        async ValueTask IterateFirst() {
            await foreach (var item in sequence) {
                channel.Writer.TryWrite(item);
            }
        }

        async ValueTask IterateSecond() {
            await foreach (var item in sequence) {
                channel.Writer.TryWrite(item);
            }
        }

        var task1 = IterateFirst();
        var task2 = IterateSecond();

        try {
            try {
                while (true) {
                    var canRead = await channel.Reader.WaitToReadAsync();

                    if (!canRead) {
                        break;
                    }

                    if (!channel.Reader.TryRead(out var item)) {
                        break;
                    }

                    yield return item;
                }
            }
            finally {
                await task1;
            }
        }
        finally {
            await task2;
        }
    }

    private static async IAsyncEnumerable<T> ParallelConcatHelper<T>(this IAsyncEnumerable<T> sequence, IAsyncEnumerable<T> other) {
        var channel = Channel.CreateUnbounded<T>();

        async Task IterateFirst() {
            await foreach (var item in sequence) {
                channel.Writer.TryWrite(item);
            }
        }

        async Task IterateSecond() {
            await foreach (var item in sequence) {
                channel.Writer.TryWrite(item);
            }
        }

        var task1 = Task.Run(IterateFirst);
        var task2 = Task.Run(IterateSecond);

        try {
            try {
                while (true) {
                    var canRead = await channel.Reader.WaitToReadAsync();

                    if (!canRead) {
                        break;
                    }

                    if (!channel.Reader.TryRead(out var item)) {
                        break;
                    }

                    yield return item;
                }
            }
            finally {
                await task1;
            }
        }
        finally {
            await task2;
        }
    }


    private class ConcatOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;
        private readonly IAsyncEnumerableOperator<T> other;

        public ConcatOperator(IAsyncEnumerableOperator<T> parent, IAsyncEnumerableOperator<T> other) {
            this.parent = parent;
            this.other = other;
        }

        public int Count => (this.parent.Count < 0 || this.other.Count < 0) ? -1 : this.parent.Count + this.other.Count;

        public AsyncExecutionMode ExecutionMode => (AsyncExecutionMode)Math.Max((int)this.parent.ExecutionMode, (int)this.other.ExecutionMode);

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Parallel) {
                return ParallelConcatHelper(this.parent, this.other).GetAsyncEnumerator(cancellationToken);
            }
            else if (this.ExecutionMode == AsyncExecutionMode.Concurrent) {
                return ConcurrentConcatHelper(this.parent, this.other).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return ConcatHelper(this.parent, this.other).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}