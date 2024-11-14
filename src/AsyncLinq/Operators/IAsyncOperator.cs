namespace AsyncLinq.Operators;

internal interface IAsyncOperator<out T> : IAsyncEnumerable<T> {
    public AsyncOperatorParams Params { get; }
}

internal enum AsyncExecutionMode {
    Sequential = 0,
    Concurrent = 1,
    Parallel = 2
}

internal readonly record struct AsyncOperatorParams {
    public readonly AsyncExecutionMode ExecutionMode;
    public readonly bool IsUnordered;

    public AsyncOperatorParams(AsyncExecutionMode executionMode, bool isUnordered) {
        ExecutionMode = executionMode;
        IsUnordered = isUnordered;
    }

    public AsyncOperatorParams WithExecution(AsyncExecutionMode execution) {
        return new AsyncOperatorParams(execution, IsUnordered);
    }

    public AsyncOperatorParams WithIsUnordered(bool isUnordered) {
        return new AsyncOperatorParams(ExecutionMode, isUnordered);
    }
}