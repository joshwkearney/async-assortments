
using AsyncAssortments.Linq;

namespace AsyncAssortments.Operators;

internal class WrapAsyncFuncOperator<T> : IAsyncOperator<T>, ISelectOperator<T>, IAsyncSelectOperator<T>,
    ISkipTakeOperator<T>, ICountOperator<T>, IToListOperator<T>, IToSortedSetOperator<T>, IToHashSetOperator<T>,
    IOrderByOperator<T>, IOrderedAsyncEnumerable<T> {

    private readonly Func<CancellationToken, ValueTask<T>> func;
    
    public AsyncEnumerableScheduleMode ScheduleMode { get; }
    
    public IAsyncEnumerable<T> Source => this;
    
    public IComparer<T> Comparer => Comparer<T>.Default;

    public WrapAsyncFuncOperator(AsyncEnumerableScheduleMode pars, Func<CancellationToken, ValueTask<T>> func) {
        this.ScheduleMode = pars;
        
        if (this.ScheduleMode.IsParallel()) {
            this.func = token => new ValueTask<T>(Task.Run(() => func(token).AsTask(), token));
        }
        else {
            this.func = func;
        }
    }

    public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars) {
        return new WrapAsyncFuncOperator<T>(pars, this.func);
    }

    public async ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default) {
        var item = await this.func(cancellationToken);

        return [item];
    }

    public async ValueTask<SortedSet<T>> ToSortedSetAsync(CancellationToken cancellationToken = default) {
        var item = await this.func(cancellationToken);

        return [item];
    }

    public async ValueTask<HashSet<T>> ToHashSetAsync(CancellationToken cancellationToken = default) {
        var item = await this.func(cancellationToken);

        return [item];
    }

    public IAsyncEnumerable<E> Select<E>(Func<T, E> selector) {
        return new WrapAsyncFuncOperator<E>(this.ScheduleMode, async c => selector(await this.func(c)));
    }

    public IAsyncEnumerable<G> AsyncSelect<G>(Func<T, CancellationToken, ValueTask<G>> nextSelector) {
        return new WrapAsyncFuncOperator<G>(this.ScheduleMode, async c => await nextSelector(await this.func(c), c));
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
    
    public IOrderedAsyncEnumerable<T> OrderBy(IComparer<T> comparer) {
        return this;
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        yield return await this.func(cancellationToken);
    }
}