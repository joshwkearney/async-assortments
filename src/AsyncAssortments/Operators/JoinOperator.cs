using System.Diagnostics;
using System.Threading.Channels;

namespace AsyncAssortments.Operators;

internal class JoinOperator<T, E, TKey> : IAsyncOperator<(T first, E second)> {
    private readonly IAsyncEnumerable<T> source;
    private readonly IAsyncEnumerable<E> other;
    private readonly Func<T, TKey> keySelector1;
    private readonly Func<E, TKey> keySelector2;
    private readonly IEqualityComparer<TKey> comparer;

    public AsyncEnumerableScheduleMode ScheduleMode { get; }

    public JoinOperator(
        AsyncEnumerableScheduleMode mode,
        IAsyncEnumerable<T> source,
        IAsyncEnumerable<E> other,
        Func<T, TKey> keySelector1,
        Func<E, TKey> keySelector2,
        IEqualityComparer<TKey> comparer) {

        this.ScheduleMode = mode;
        this.source = source;
        this.other = other;
        this.comparer = comparer;
        this.keySelector1 = keySelector1;
        this.keySelector2 = keySelector2;
    }

    public IAsyncEnumerator<(T first, E second)> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        if (this.ScheduleMode == AsyncEnumerableScheduleMode.Sequential) {
            return this.SequentialIterator(cancellationToken);
        }
        else {
            return this.ConcurrentIterator(cancellationToken);
        }
    }

    public async IAsyncEnumerator<(T first, E second)> SequentialIterator(CancellationToken cancellationToken) {
        var dict = new Dictionary<TKey, List<T>>(this.comparer);

        await foreach (var item in this.source.WithCancellation(cancellationToken)) {
            var key = this.keySelector1(item);

            if (!dict.ContainsKey(key)) {
                dict[key] = new List<T>(1);
            }

            dict[key].Add(item);
        }

        await foreach (var item in this.other.WithCancellation(cancellationToken)) {
            var key = this.keySelector2(item);

            if (!dict.ContainsKey(key)) {
                continue;
            }

            foreach (var first in dict[key]) {
                yield return (first, item);
            }
        }
    }

    public async IAsyncEnumerator<(T first, E second)> ConcurrentIterator(CancellationToken cancellationToken) {
        var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var channel = Channel.CreateUnbounded<ConcurrentJoinRecord>();
        var errors = new ErrorCollection();
        var finishedTasks = 0;

        var secondLists = new Dictionary<TKey, List<E>>(this.comparer);
        var firstLists = new Dictionary<TKey, List<T>>(this.comparer);

        var task1 = IterateFirst();
        var task2 = IterateSecond();

        while (true) {
            var canRead = await channel.Reader.WaitToReadAsync();

            if (!canRead) {
                break;
            }

            if (!channel.Reader.TryRead(out var record)) {
                break;
            }

            if (record.HasFirst) {
                if (secondLists.TryGetValue(record.Key, out var secondList)) {
                    foreach (var second in secondList) {
                        yield return (record.First, second);
                    }
                }

                if (firstLists.TryGetValue(record.Key, out var firstList)) {
                    firstList.Add(record.First);
                }
                else {
                    firstLists[record.Key] = new List<T>(1) { record.First };
                }
            }
            else {
                if (firstLists.TryGetValue(record.Key, out var firstList)) {
                    foreach (var first in firstList) {
                        yield return (first, record.Second);
                    }
                }

                if (secondLists.TryGetValue(record.Key, out var secondList)) {
                    secondList.Add(record.Second);
                }
                else {
                    secondLists[record.Key] = new List<E>(1) { record.Second };
                }
            }
        }

        try {
            await task1;
        }
        catch (Exception ex) {
            errors.Add(ex);
        }

        try {
            await task2;
        }
        catch (Exception ex) {
            errors.Add(ex);
        }

        var totalError = errors.ToException();

        if (totalError != null) {
            throw totalError;
        }

        async ValueTask IterateFirst() {
            try {
                await foreach (var first in this.source.WithCancellation(tokenSource.Token)) {
                    var key = this.keySelector1(first);
                    var record = new ConcurrentJoinRecord(key, first, default!, true);

                    channel.Writer.TryWrite(record);
                }
            }
            catch {
                tokenSource.Cancel();
                throw;
            }
            finally {
                if (Interlocked.Increment(ref finishedTasks) == 2) {
                    channel.Writer.Complete();
                }
            }
        }

        async ValueTask IterateSecond() {
            try {
                await foreach (var second in this.other.WithCancellation(tokenSource.Token)) {
                    var key = this.keySelector2(second);
                    var record = new ConcurrentJoinRecord(key, default!, second, false);

                    channel.Writer.TryWrite(record);
                }
            }
            catch {
                tokenSource.Cancel();
                throw;
            }
            finally {
                if (Interlocked.Increment(ref finishedTasks) == 2) {
                    channel.Writer.Complete();
                }
            }
        }
    }

    public IAsyncOperator<(T first, E second)> WithScheduleMode(AsyncEnumerableScheduleMode pars) {
        return new JoinOperator<T, E, TKey>(
            pars,
            this.source,
            this.other,
            this.keySelector1,
            this.keySelector2,
            this.comparer);
    }

    private record struct ConcurrentJoinRecord(TKey Key, T First, E Second, bool HasFirst);
}