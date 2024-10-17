using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    private static readonly UnboundedChannelOptions channelOptions = new() {
        AllowSynchronousContinuations = true,
        SingleReader = false,
        SingleWriter = false
    };

    public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> sequence, IAsyncEnumerable<T> other) {
        if (sequence is IAsyncEnumerableOperator<T> col1) {
            return new ConcatOperator<T>(col1, other);
        }

        return ConcatHelper(sequence, other);
    }

    public static IAsyncEnumerable<T> Concat<T>(this IAsyncEnumerable<T> sequence, IEnumerable<T> other) {
        return sequence.Concat(other.AsAsyncEnumerable());
    }

    private static async IAsyncEnumerable<T> ConcatHelper<T>(
        this IAsyncEnumerable<T> sequence,
        IAsyncEnumerable<T> other,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        // Get the enumerators first so subscribe works correctly
        var iterator1 = sequence.WithCancellation(cancellationToken).GetAsyncEnumerator();
        var moveTask1 = iterator1.MoveNextAsync();

        var iterator2 = other.WithCancellation(cancellationToken).GetAsyncEnumerator();
        var moveTask2 = iterator2.MoveNextAsync();

        while (await moveTask1) {
            yield return iterator1.Current;
            moveTask1 = iterator1.MoveNextAsync();
        }

        while (await moveTask2) {
            yield return iterator2.Current;
            moveTask2 = iterator2.MoveNextAsync();
        }
    }

    private static async IAsyncEnumerable<T> ConcurrentConcatHelper<T>(
        this IAsyncEnumerable<T> sequence,
        IAsyncEnumerable<T> other,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        var channel = Channel.CreateUnbounded<T>(channelOptions);
        var channelCompleteLock = new object();
        var firstFinished = false;
        var secondFinished = false;

        async ValueTask IterateFirst() {
            await foreach (var item in sequence.WithCancellation(cancellationToken)) {
                channel.Writer.TryWrite(item);
            }

            lock (channelCompleteLock) {
                firstFinished = true;

                if (firstFinished && secondFinished) {
                    channel.Writer.Complete();
                }
            }
        }

        async ValueTask IterateSecond() {
            await foreach (var item in other.WithCancellation(cancellationToken)) {
                channel.Writer.TryWrite(item);
            }

            lock (channelCompleteLock) {
                secondFinished = true;

                if (firstFinished && secondFinished) {
                    channel.Writer.Complete();
                }
            }
        }

        var task1 = IterateFirst();
        var task2 = IterateSecond();

        try {
            try {
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
        private readonly IAsyncEnumerable<T> other;

        public ConcatOperator(IAsyncEnumerableOperator<T> parent, IAsyncEnumerable<T> other) {
            this.parent = parent;
            this.other = other;
        }

        public AsyncExecutionMode ExecutionMode => this.parent.ExecutionMode;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.ExecutionMode == AsyncExecutionMode.Sequential) {
                return ConcatHelper(this.parent, this.other).GetAsyncEnumerator(cancellationToken);
            }
            else {
                return ConcurrentConcatHelper(this.parent, this.other).GetAsyncEnumerator(cancellationToken);
            }
        }
    }
}