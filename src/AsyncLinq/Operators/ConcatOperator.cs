using System.Threading.Channels;

namespace AsyncLinq.Operators {
    internal interface IConcatOperator<T> : IAsyncOperator<T> {
        public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequences);
    }

    internal class ConcatOperator<T> : IAsyncOperator<T>, IConcatOperator<T>, IEnumerableConcatOperator<T> {
        private readonly IEnumerable<IAsyncEnumerable<T>> sequences;

        public AsyncOperatorParams Params { get; }

        public ConcatOperator(IEnumerable<IAsyncEnumerable<T>> sequences, AsyncOperatorParams pars) {
            this.sequences = sequences;
            this.Params = pars;
        }

        public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequences) {
            var seqs = this.sequences.Append(sequences);

            return new ConcatOperator<T>(seqs, this.Params);
        }

        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
            var seqs = this.sequences.Prepend(before.AsAsyncEnumerable()).Append(after.AsAsyncEnumerable());

            return new ConcatOperator<T>(seqs, this.Params);
        }
        
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.Params.IsUnordered) {
                return this.UnorderedHelper(cancellationToken);
            }
            else {
                return this.SequentialHelper(cancellationToken);
            }
        }

        public async IAsyncEnumerator<T> SequentialHelper(CancellationToken cancellationToken) {
            foreach (var sequence in this.sequences) {
                await foreach (var item in sequence) {
                    yield return item;
                }
            }
        }

        public async IAsyncEnumerator<T> UnorderedHelper(CancellationToken cancellationToken) {
            var channel = Channel.CreateUnbounded<T>();
            int completion = this.sequences.Count();

            // ToArray here so the tasks are created. Otherwise it's lazy and we'll deadlock
            var tasks = this.sequences.Select(UnorderedIterateHelper).ToArray();

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

            // We have to await the tasks for exception handling purposes
            await Task.WhenAll(tasks);

            async Task UnorderedIterateHelper(IAsyncEnumerable<T> seq) {
                try {
                    await foreach (var item in seq.WithCancellation(cancellationToken)) {
                        channel.Writer.TryWrite(item);
                    }
                }
                finally {
                    int left = Interlocked.Decrement(ref completion);

                    if (left == 0) {
                        channel.Writer.Complete();
                    }
                }
            }
        }
    }
}
