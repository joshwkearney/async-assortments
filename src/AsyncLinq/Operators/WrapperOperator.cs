namespace AsyncCollections.Linq.Operators;

internal class WrapperOperator<T> : IAsyncOperator<T> {
    public IAsyncEnumerable<T> Parent { get; }

    public AsyncEnumerableScheduleMode ScheduleMode { get; }

    public WrapperOperator(AsyncEnumerableScheduleMode pars, IAsyncEnumerable<T> parent) {
        this.Parent = parent;
        this.ScheduleMode = pars;
    }

    public IAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode pars) {
        return new WrapperOperator<T>(pars, this.Parent);
    }
        
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        return this.Parent.GetAsyncEnumerator(cancellationToken);
    }
}