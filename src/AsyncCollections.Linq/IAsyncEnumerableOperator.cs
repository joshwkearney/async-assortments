namespace AsyncCollections;

internal interface IAsyncEnumerableOperator<out T> : IAsyncEnumerable<T> {
    public AsyncExecutionMode ExecutionMode { get; }
}

internal enum AsyncExecutionMode {
    Sequential = 0,
    Concurrent = 1,
    Parallel = 2
}