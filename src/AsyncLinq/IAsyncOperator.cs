namespace AsyncLinq;

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
        this.ExecutionMode = executionMode;
        this.IsUnordered = isUnordered;
    }

    public AsyncOperatorParams WithExecution(AsyncExecutionMode execution) {
        return new AsyncOperatorParams(execution, this.IsUnordered);
    }

    public AsyncOperatorParams WithIsUnordered(bool isUnordered) {
        return new AsyncOperatorParams(this.ExecutionMode, isUnordered);
    }
}