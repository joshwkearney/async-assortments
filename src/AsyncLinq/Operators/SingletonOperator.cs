namespace AsyncCollections.Linq.Operators;

internal class SingletonOperator<T> : IScheduledAsyncOperator<T> {
    private readonly Func<CancellationToken, ValueTask<T>> func;
    
    public AsyncEnumerableScheduleMode ScheduleMode { get; }

    public SingletonOperator(AsyncEnumerableScheduleMode pars, Func<CancellationToken, ValueTask<T>> func) {
        this.ScheduleMode = pars;
        this.func = func;
    }

    public IScheduledAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode pars) {
        return new SingletonOperator<T>(pars, this.func);
    }
    
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        if (this.ScheduleMode.IsParallel()) {
            yield return await Task.Run(() => this.func(cancellationToken).AsTask(), cancellationToken);
        }
        else {
            yield return await this.func(cancellationToken);
        }
    }
}