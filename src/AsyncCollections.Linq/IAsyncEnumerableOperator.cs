using System.Collections.Generic;

namespace CollectionTesting;

internal interface IAsyncEnumerableOperator<T> : IAsyncEnumerable<T> {
    public int Count { get; }

    public AsyncExecutionMode ExecutionMode { get; }
}

internal enum AsyncExecutionMode {
    Sequential = 0,
    Concurrent = 1,
    Parallel = 2
}