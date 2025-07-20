namespace AsyncAssortments.Operators;

internal class WrapAsyncEnumerableOperator<T> : IAsyncOperator<T> {
    public IAsyncEnumerable<T> Parent { get; }

    public AsyncEnumerableScheduleMode ScheduleMode { get; }

    public int MaxConcurrency { get; }

    public WrapAsyncEnumerableOperator(AsyncEnumerableScheduleMode pars, int maxConcurrency, IAsyncEnumerable<T> parent) {
        this.Parent = parent;
        this.ScheduleMode = pars;
        this.MaxConcurrency = maxConcurrency;
    }

    public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars, int maxConcurrency) {
        return new WrapAsyncEnumerableOperator<T>(pars, maxConcurrency, this.Parent);
    }
        
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        return this.Parent.GetAsyncEnumerator(cancellationToken);
    }
}