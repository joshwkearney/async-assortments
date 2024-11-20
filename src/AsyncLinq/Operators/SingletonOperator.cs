namespace AsyncLinq.Operators;

internal class SingletonOperator<T> : IAsyncOperator<T> {
    private readonly Func<CancellationToken, ValueTask<T>> func;
    
    public AsyncPipelineExecution Execution { get; }

    public SingletonOperator(AsyncPipelineExecution pars, Func<CancellationToken, ValueTask<T>> func) {
        this.Execution = pars;
        this.func = func;
    }

    public IAsyncOperator<T> WithExecution(AsyncPipelineExecution pars) {
        return new SingletonOperator<T>(pars, this.func);
    }
    
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        if (this.Execution.IsParallel()) {
            yield return await Task.Run(() => this.func(cancellationToken).AsTask(), cancellationToken);
        }
        else {
            yield return await this.func(cancellationToken);
        }
    }
}