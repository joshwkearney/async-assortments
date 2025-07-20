
using AsyncAssortments.Linq;

namespace AsyncAssortments.Operators;

internal class WrapAsyncFuncOperator<T> : IAsyncOperator<T>, ISelectOperator<T>, IAsyncSelectOperator<T>,
    ISkipTakeOperator<T>, ICountOperator<T>, IToListOperator<T>, IToHashSetOperator<T>,
    IOrderOperator<T>, IOrderedAsyncEnumerable<T> {

    private readonly Func<CancellationToken, ValueTask<T>> func;
    
    public AsyncEnumerableScheduleMode ScheduleMode { get; }
    
    public IAsyncEnumerable<T> Source => this;
    
    public IComparer<T> Comparer => Comparer<T>.Default;

    public int MaxConcurrency { get; }

    public WrapAsyncFuncOperator(AsyncEnumerableScheduleMode pars, int maxConcurrency, Func<CancellationToken, ValueTask<T>> func) {
        this.ScheduleMode = pars;
        this.MaxConcurrency = maxConcurrency;

        if (this.ScheduleMode.IsParallel()) {
            this.func = token => new ValueTask<T>(Task.Run(() => func(token).AsTask(), token));
        }
        else {
            this.func = func;
        }
    }

    public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars, int maxConcurrency) {
        return new WrapAsyncFuncOperator<T>(pars, maxConcurrency, this.func);
    }

    public async ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default) {
        var item = await this.func(cancellationToken);

        return [item];
    }

    public async ValueTask<HashSet<T>> ToHashSetAsync(IEqualityComparer<T> comparer, CancellationToken cancellationToken = default) {
        var item = await this.func(cancellationToken);

        return new HashSet<T>(comparer) { item };
    }

    public IAsyncEnumerable<E> Select<E>(Func<T, E> selector) {
        return new WrapAsyncFuncOperator<E>(this.ScheduleMode, this.MaxConcurrency, async c => selector(await this.func(c)));
    }

    public IAsyncEnumerable<G> AsyncSelect<G>(Func<T, CancellationToken, ValueTask<G>> nextSelector) {
        return new WrapAsyncFuncOperator<G>(this.ScheduleMode, this.MaxConcurrency, async c => await nextSelector(await this.func(c), c));
    }

    public IAsyncEnumerable<T> SkipTake(int skip, int take) {
        if (skip > 0 || take <= 0) {
            return AsyncEnumerable.Empty<T>();
        }
        else {
            return this;
        }        
    }

    public int Count() => 1;
    
    public IOrderedAsyncEnumerable<T> Order(IComparer<T> comparer) {
        return this;
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        yield return await this.func(cancellationToken);
    }
}