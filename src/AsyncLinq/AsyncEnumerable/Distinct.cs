using System.Runtime.CompilerServices;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> Distinct<TResult>(this IAsyncEnumerable<TResult> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncLinqOperator<TResult> op) {
            return new DistinctOperator<TResult>(op);
        }

        return DistinctHelper(source);
    }

    private static async IAsyncEnumerable<T> DistinctHelper<T>(
        this IAsyncEnumerable<T> sequence, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        var set = new HashSet<T>();

        await foreach (var item in sequence.WithCancellation(cancellationToken)) {
            if (set.Add(item)) {
                yield return item;
            }
        }
    }

    private class DistinctOperator<T> : IAsyncLinqOperator<T> {
        private readonly IAsyncLinqOperator<T> parent;

        public int Count => -1;

        public AsyncLinqExecutionMode ExecutionMode { get; }

        public DistinctOperator(IAsyncLinqOperator<T> parent) {
            this.parent = parent;
            this.ExecutionMode = parent.ExecutionMode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return DistinctHelper(this.parent).GetAsyncEnumerator(cancellationToken);
        }
    }
}