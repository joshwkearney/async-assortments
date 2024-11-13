using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    private static readonly UnboundedChannelOptions channelOptions = new() {
        AllowSynchronousContinuations = true,
        SingleReader = false,
        SingleWriter = false
    };

    public static IAsyncEnumerable<TSource> Concat<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }

        // TODO: If second is an IEnumerable, return a different operator

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new ConcatOperator<TSource>(source, second, pars);
    }

    public static IAsyncEnumerable<TSource> Concat<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IEnumerable<TSource> second) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }

        // TODO: If source is an ienumerble, return a different operator

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }

        return new EnumerableConcatOperator<TSource>(source, second, pars);
    }

    private class EnumerableConcatOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly IEnumerable<T> other;

        public AsyncOperatorParams Params { get; }

        public EnumerableConcatOperator(
            IAsyncEnumerable<T> parent, 
            IEnumerable<T> other, 
            AsyncOperatorParams pars) {

            this.parent = parent;
            this.other = other;
            this.Params = pars;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.Params.IsUnordered) {
                return this.UnorderedHelper(cancellationToken);
            }
            else {
                return this.OrderedHelper(cancellationToken);
            }
        }

        private async IAsyncEnumerator<T> UnorderedHelper(CancellationToken cancellationToken) {
            await using var iterator = this.parent.GetAsyncEnumerator(cancellationToken);
            var nextTask = iterator.MoveNextAsync();

            // Yield as many from the parent synchronously as we can
            while (nextTask.IsCompletedSuccessfully) {
                yield return iterator.Current;
                nextTask = iterator.MoveNextAsync();
            }

            // Yield all of the enumerable, which we don't have to wait on
            foreach (var item in this.other) {
                yield return item;
            }

            // Yield all the rest of the parent asynchronously
            while (await nextTask) {
                yield return iterator.Current;
                nextTask = iterator.MoveNextAsync();
            }
        }

        private async IAsyncEnumerator<T> OrderedHelper(CancellationToken cancellationToken) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                yield return item;
            }

            foreach (var item in this.other) {
                yield return item;
            }
        }
    }

    private class ConcatOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly IAsyncEnumerable<T> other;

        public AsyncOperatorParams Params { get; }

        public ConcatOperator(
            IAsyncEnumerable<T> parent, 
            IAsyncEnumerable<T> other, 
            AsyncOperatorParams pars) {

            this.parent = parent;
            this.other = other;
            this.Params = pars;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.Params.IsUnordered) {
                return this.UnorderedHelper(cancellationToken);
            }
            else {
                return this.OrderedHelper(cancellationToken);
            }
        }

        private async IAsyncEnumerator<T> OrderedHelper(CancellationToken cancellationToken) {
            await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
                yield return item;
            }

            await foreach (var item in this.other.WithCancellation(cancellationToken)) {
                yield return item;
            }
        }

        private async IAsyncEnumerator<T> UnorderedHelper(CancellationToken cancellationToken) {
            var channel = Channel.CreateUnbounded<T>(channelOptions);
            var channelCompleteLock = new object();
            var firstFinished = false;
            var secondFinished = false;

            async ValueTask IterateFirst() {
                await foreach (var item in this.parent.WithCancellation(cancellationToken)) {
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
                await foreach (var item in this.other.WithCancellation(cancellationToken)) {
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
    }
}