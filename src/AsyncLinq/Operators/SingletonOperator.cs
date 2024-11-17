namespace AsyncLinq.Operators;

internal class SingletonOperator<T> : IAsyncOperator<T> {
    private readonly Func<CancellationToken, ValueTask<T>> func;
    
    public AsyncOperatorParams Params { get; }

    public SingletonOperator(AsyncOperatorParams pars, Func<CancellationToken, ValueTask<T>> func) {
        this.Params = pars;
        this.func = func;
    }

    public IAsyncOperator<T> WithParams(AsyncOperatorParams pars) {
        return new SingletonOperator<T>(pars, this.func);
    }
    
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        if (this.Params.ExecutionMode == AsyncExecutionMode.Parallel) {
            yield return await Task.Run(() => this.func(cancellationToken).AsTask(), cancellationToken);
        }
        else {
            yield return await this.func(cancellationToken);
        }
    }
}