﻿using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> Distinct<TResult>(this IAsyncEnumerable<TResult> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IAsyncEnumerableOperator<TResult> op) {
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

    private class DistinctOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerableOperator<T> parent;

        public int Count => -1;

        public AsyncExecutionMode ExecutionMode { get; }

        public DistinctOperator(IAsyncEnumerableOperator<T> parent) {
            this.parent = parent;
            this.ExecutionMode = parent.ExecutionMode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return DistinctHelper(this.parent).GetAsyncEnumerator(cancellationToken);
        }
    }
}