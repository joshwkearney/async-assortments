namespace AsyncLinq.Operators;

internal class WrapperOperator<T> : IAsyncOperator<T> {
    public IAsyncEnumerable<T> Parent { get; }

    public AsyncOperatorParams Params { get; }

    public WrapperOperator(AsyncOperatorParams pars, IAsyncEnumerable<T> parent) {
        this.Parent = parent;
        this.Params = pars;
    }

    public IAsyncOperator<T> WithParams(AsyncOperatorParams pars) {
        return new WrapperOperator<T>(pars, this.Parent);
    }
        
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        return this.Parent.GetAsyncEnumerator(cancellationToken);
    }
}