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
            
        var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        return new CancellableAsyncEnumerator<(T first, E second)>(cancelSource, this.ConcurrentIterator(cancelSource));
    }

    private async IAsyncEnumerator<(T first, E second)> SequentialIterator(CancellationToken cancellationToken) {
        // We're going to make sure no nulls are put in this dictionary, so the warning can be disabled
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        var dict = new Dictionary<NullableKeyWrapper<TKey>, List<T>>(new NullableKeyWrapperComparer<TKey>(this.comparer));
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

        await foreach (var item in this.source.WithCancellation(cancellationToken)) {
            var key = this.keySelector1(item);

            if (!dict.ContainsKey(key)) {
                dict[key] = new List<T>(1);
            }

            dict[key].Add(item);
        }

        await foreach (var item in this.other.WithCancellation(cancellationToken)) {
            var key = this.keySelector2(item);

            if (!dict.TryGetValue(key, out var list)) {
                continue;
            }

            foreach (var first in list) {
                yield return (first, item);
            }
        }
    }

    private async IAsyncEnumerator<(T first, E second)> ConcurrentIterator(CancellationTokenSource cancelSource) {
        var channel = Channel.CreateUnbounded<ConcurrentJoinRecord>();
        var errors = new ErrorCollection();
        var finishedTasks = 0;

        // We're going to make sure no nulls are put in this dictionary, so the warning can be disabled
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        var secondLists = new Dictionary<NullableKeyWrapper<TKey>, List<E>>(new NullableKeyWrapperComparer<TKey>(this.comparer));
        var firstLists = new Dictionary<NullableKeyWrapper<TKey>, List<T>>(new NullableKeyWrapperComparer<TKey>(this.comparer));
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

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
                await foreach (var first in this.source.WithCancellation(cancelSource.Token)) {
                    var key = this.keySelector1(first);
                    var record = new ConcurrentJoinRecord(key, first, default!, true);

                    channel.Writer.TryWrite(record);
                }
            }
            catch {
                cancelSource.Cancel();
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
                await foreach (var second in this.other.WithCancellation(cancelSource.Token)) {
                    var key = this.keySelector2(second);
                    var record = new ConcurrentJoinRecord(key, default!, second, false);
                    
                    channel.Writer.TryWrite(record);
                }
            }
            catch {
                cancelSource.Cancel();
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