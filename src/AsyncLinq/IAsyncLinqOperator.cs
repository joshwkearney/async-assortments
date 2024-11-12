namespace AsyncLinq;

internal interface IAsyncLinqOperator<out T> : IAsyncEnumerable<T> {
    public AsyncLinqExecutionMode ExecutionMode { get; }
}

internal enum AsyncLinqExecutionMode {
    Sequential = 0,
    Concurrent = 1,
    Parallel = 2
}