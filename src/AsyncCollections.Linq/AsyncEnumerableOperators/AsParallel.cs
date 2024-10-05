using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static IAsyncEnumerable<T> AsParallel<T>(this IAsyncEnumerable<T> sequence) {
        if (sequence is IAsyncEnumerableOperator<T> op && op.ExecutionMode == AsyncExecutionMode.Parallel) {
            return sequence;
        }

        return new AsParallelOperator<T>(sequence, AsyncExecutionMode.Parallel);
    }

    public static IAsyncEnumerable<T> AsConcurrent<T>(this IAsyncEnumerable<T> sequence) {
        if (sequence is IAsyncEnumerableOperator<T> op && op.ExecutionMode == AsyncExecutionMode.Concurrent) {
            return sequence;
        }

        return new AsParallelOperator<T>(sequence, AsyncExecutionMode.Concurrent);
    }

    public static IAsyncEnumerable<T> AsSequential<T>(this IAsyncEnumerable<T> sequence) {
        if (sequence is IAsyncEnumerableOperator<T> op && op.ExecutionMode == AsyncExecutionMode.Sequential) {
            return sequence;
        }

        return new AsParallelOperator<T>(sequence, AsyncExecutionMode.Sequential);
    }

    private class AsParallelOperator<T> : IAsyncEnumerableOperator<T> {
        private readonly IAsyncEnumerable<T> parent;

        public AsyncExecutionMode ExecutionMode { get; }

        public AsParallelOperator(IAsyncEnumerable<T> parent, AsyncExecutionMode mode) {
            this.parent = parent;
            this.ExecutionMode = mode;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return this.parent.GetAsyncEnumerator(cancellationToken);
        }
    }
}