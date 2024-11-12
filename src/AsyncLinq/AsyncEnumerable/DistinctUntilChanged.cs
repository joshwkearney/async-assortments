using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> DistinctUntilChanged<TSource>(this IAsyncEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncLinqOperator<TSource> op) {
            return new DistinctUntilChangedOperator<TSource>(op);
        }

        return DistinctUntilChangedHelper(source);
    }

    private static async IAsyncEnumerable<T> DistinctUntilChangedHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        await using var iterator = sequence.GetAsyncEnumerator(cancellationToken);
        var hasFirst = await iterator.MoveNextAsync();

        if (!hasFirst) {
            yield break;
        }

        var current = iterator.Current;
        yield return current;

        while (await iterator.MoveNextAsync()) {
            var item = iterator.Current;

            if (EqualityComparer<T>.Default.Equals(item, current)) {
                continue;
            }

            current = item;
            yield return current;
        }
    }

    private class DistinctUntilChangedOperator<T> : IAsyncLinqOperator<T> {
        private readonly IAsyncLinqOperator<T> parent;

        public int Count => -1;

        public AsyncLinqExecutionMode ExecutionMode { get; }

        public DistinctUntilChangedOperator(IAsyncLinqOperator<T> parent) {
            this.parent = parent;
            this.ExecutionMode = parent.ExecutionMode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return DistinctUntilChangedHelper(this.parent).GetAsyncEnumerator(cancellationToken);
        }
    }
}