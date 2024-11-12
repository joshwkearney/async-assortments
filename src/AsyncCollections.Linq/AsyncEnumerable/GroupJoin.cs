//using System.Runtime.CompilerServices;
//using System.Threading.Channels;

//namespace AsyncCollections.Linq;

//public static partial class AsyncEnumerable {
//    public static IAsyncEnumerable<TResult> GroupJoin<T, E, TKey, TResult>(
//        this IAsyncEnumerable<T> sequence,
//        IAsyncEnumerable<E> other,
//        Func<T, TKey> keySelector1,
//        Func<E, TKey> keySelector2,
//        Func<T, IEnumerable<E>, TResult> resultSelector) where TKey : notnull {

//        if (sequence == null) {
//            throw new ArgumentNullException(nameof(sequence));
//        }

//        if (other == null) {
//            throw new ArgumentNullException(nameof(other));
//        }

//        if (keySelector1 == null) {
//            throw new ArgumentNullException(nameof(keySelector1));
//        }

//        if (keySelector2 == null) {
//            throw new ArgumentNullException(nameof(keySelector2));
//        }

//        if (resultSelector == null) {
//            throw new ArgumentNullException(nameof(resultSelector));
//        }

//        if (sequence is IAsyncEnumerableOperator<T> op) {
//            return new GroupJoinOperator<T, E, TKey, TResult>(op, other, keySelector1, keySelector2, resultSelector);
//        }

//        return GroupJoinHelper(sequence, other, keySelector1, keySelector2, resultSelector);
//    }

//    private static async IAsyncEnumerable<TResult> GroupJoinHelper<T, E, TKey, TResult>(
//        this IAsyncEnumerable<T> sequence, 
//        IAsyncEnumerable<E> other,
//        Func<T, TKey> keySelector1,
//        Func<E, TKey> keySelector2,
//        Func<T, IEnumerable<E>, TResult> resultSelector,
//        [EnumeratorCancellation] CancellationToken cancellationToken = default) where TKey : notnull {

//        var selected = new Dictionary<TKey, List<E>>();

//        // Start to enumerate the first iterator
//        await using var firstIterator = sequence.GetAsyncEnumerator(cancellationToken);
//        var nextTask = firstIterator.MoveNextAsync();

//        await foreach (var second in other) {
//            var key = keySelector2(second);

//            if (!selected.TryGetValue(key, out var list)) {
//                list = selected[key] = new List<E>(1);
//            }

//            list.Add(second);
//        }

//        while (await nextTask) {
//            var first = firstIterator.Current;
//            var key = keySelector1(first);

//            if (selected.TryGetValue(key, out var list)) {
//                yield return resultSelector(first, list);
//            }
//            else {
//                yield return resultSelector(first, []);
//            }

//            nextTask = firstIterator.MoveNextAsync();
//        }
//    }

//    private class GroupJoinOperator<T, E, TKey, TResult> : IAsyncEnumerableOperator<TResult> where TKey: notnull {
//        private readonly IAsyncEnumerableOperator<T> parent;
//        private readonly IAsyncEnumerable<E> other;
//        private readonly Func<T, TKey> parentKeySelector;
//        private readonly Func<E, TKey> otherKeySelector;
//        private readonly Func<T, IEnumerable<E>, TResult> resultSelector;

//        public AsyncExecutionMode ExecutionMode { get; }

//        public GroupJoinOperator(
//            IAsyncEnumerableOperator<T> parent, 
//            IAsyncEnumerable<E> other, 
//            Func<T, TKey> parentKeySelector, 
//            Func<E, TKey> otherKeySelector,
//            Func<T, IEnumerable<E>, TResult> resultSelector) {

//            this.parent = parent;
//            this.other = other;
//            this.parentKeySelector = parentKeySelector;
//            this.otherKeySelector = otherKeySelector;
//            this.resultSelector = resultSelector;
//            this.ExecutionMode = parent.ExecutionMode;
//        }

//        public IAsyncEnumerator<TResult> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
//            return GroupJoinHelper(this.parent, this.other, this.parentKeySelector, this.otherKeySelector, this.resultSelector).GetAsyncEnumerator(cancellationToken);
//        }
//    }
//}