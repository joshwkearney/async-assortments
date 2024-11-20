namespace AsyncLinq.Operators;

internal class WrapperOperator<T> : IAsyncOperator<T> {
    public IAsyncEnumerable<T> Parent { get; }

    public AsyncPipelineExecution Execution { get; }

    public WrapperOperator(AsyncPipelineExecution pars, IAsyncEnumerable<T> parent) {
        this.Parent = parent;
        this.Execution = pars;
    }

    public IAsyncOperator<T> WithExecution(AsyncPipelineExecution pars) {
        return new WrapperOperator<T>(pars, this.Parent);
    }
        
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        return this.Parent.GetAsyncEnumerator(cancellationToken);
    }
}