
namespace AsyncCollections.Linq.Operators;

internal class SingletonOperator<T> : IAsyncOperator<T>, IToListOperator<T> {
    private readonly Func<CancellationToken, ValueTask<T>> func;
    
    public AsyncEnumerableScheduleMode ScheduleMode { get; }

    public SingletonOperator(AsyncEnumerableScheduleMode pars, Func<CancellationToken, ValueTask<T>> func) {
        this.ScheduleMode = pars;
        
        if (this.ScheduleMode.IsParallel()) {
            this.func = token => new ValueTask<T>(Task.Run(() => func(token).AsTask(), token));
        }
        else {
            this.func = func;
        }
    }

    public IAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode pars) {
        return new SingletonOperator<T>(pars, this.func);
    }

    public async ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default) {
        var item = await this.func(cancellationToken);

        return [item];
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        yield return await this.func(cancellationToken);
    }
}