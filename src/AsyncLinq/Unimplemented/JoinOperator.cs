/*using System.Threading.Channels;

namespace AsyncLinq.Operators;

internal class JoinOperator<T, E, TKey, TResult> : IAsyncOperator<TResult> where TKey : notnull {
        private readonly IAsyncEnumerable<T> parent;
        private readonly IAsyncEnumerable<E> other;
        private readonly Func<T, TKey> parentKeySelector;
        private readonly Func<E, TKey> otherKeySelector;
        private readonly Func<T, E, TResult> resultSelector;

        public AsyncOperatorParams Params { get; }

        public JoinOperator(
            AsyncOperatorParams pars,
            IAsyncEnumerable<T> parent, 
            IAsyncEnumerable<E> other, 
            Func<T, TKey> parentKeySelector, 
            Func<E, TKey> otherKeySelector,
            Func<T, E, TResult> resultSelector) {

            this.parent = parent;
            this.other = other;
            this.parentKeySelector = parentKeySelector;
            this.otherKeySelector = otherKeySelector;
            this.resultSelector = resultSelector;
            this.Params = pars;
        }
        
        public IAsyncOperator<TResult> WithParams(AsyncOperatorParams pars) {
            return new JoinOperator<T, E, TKey, TResult>(
                pars, 
                this.parent, 
                this.other, 
                this.parentKeySelector,
                this.otherKeySelector, 
                this.resultSelector);
        }

        public async IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            var firstKeys = new Dictionary<TKey, List<T>>();
            var secondKeys = new Dictionary<TKey, List<E>>();

            var keyLock = new object();
            var channel = Channel.CreateUnbounded<TResult>();

            var channelCompleteLock = new object();
            var firstFinished = false;
            var secondFinished = false;

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

            async ValueTask IterateFirst() {
                var secondListCopy = new List<E>();

                await foreach (var first in this.parent) {
                    var key = this.parentKeySelector(first);
                    secondListCopy.Clear();

                    lock (keyLock) {
                        // Get all the items that match our key
                        if (secondKeys.TryGetValue(key, out var secondList)) {
                            secondListCopy.AddRange(secondList);
                        }

                        // Create a list for our key if it doesn't exist
                        if (!firstKeys.TryGetValue(key, out var firstList)) {
                            firstList = firstKeys[key] = new List<T>(1);
                        }

                        // Add this item to the dictionary
                        firstList.Add(first);
                    }

                    // Yield the pairs we found
                    foreach (var second in secondListCopy) {
                        channel.Writer.TryWrite(resultSelector(first, second));
                    }
                }

                lock (channelCompleteLock) {
                    firstFinished = true;

                    if (firstFinished && secondFinished) {
                        channel.Writer.Complete();
                    }
                }
            }

            async ValueTask IterateSecond() {
                var firstListCopy = new List<T>();

                await foreach (var second in other) {
                    var key = this.otherKeySelector(second);
                    firstListCopy.Clear();

                    lock (keyLock) {
                        // Get all the items that match our key
                        if (firstKeys.TryGetValue(key, out var firstList)) {
                            firstListCopy.AddRange(firstList);
                        }

                        // Create a list for our key if it doesn't exist
                        if (!secondKeys.TryGetValue(key, out var secondList)) {
                            secondList = secondKeys[key] = new List<E>(1);
                        }

                        // Add this item to the dictionary
                        secondList.Add(second);
                    }

                    // Yield the pairs we found
                    foreach (var first in firstListCopy) {
                        channel.Writer.TryWrite(resultSelector(first, second));
                    }
                }

                lock (channelCompleteLock) {
                    secondFinished = true;

                    if (firstFinished && secondFinished) {
                        channel.Writer.Complete();
                    }
                }
            }
        }
    }*/