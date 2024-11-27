namespace AsyncAssortments.Operators;

internal class EmptyOperator<T> : IAsyncOperator<T>, IAsyncSelectOperator<T>, IAsyncWhereOperator<T>, 
    IConcatOperator<T>, IConcatEnumerablesOperator<T>, ISelectOperator<T>, IWhereOperator<T>, 
    ISkipTakeOperator<T>, ICountOperator<T>, IToListOperator<T>, IToHashSetOperator<T>,
    IToSortedSetOperator<T>, IOrderByOperator<T>, IOrderedAsyncEnumerable<T> {

    public static IAsyncOperator<T> Instance { get; } = new EmptyOperator<T>();
    
    public AsyncEnumerableScheduleMode ScheduleMode => default;

    public IAsyncEnumerable<T> Source => this;

    public IComparer<T> Comparer => Comparer<T>.Default;

    private EmptyOperator() { }

    public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars) {
        return new WrapAsyncEnumerableOperator<T>(pars, this);
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        yield break;
    }

    public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence) {
        return sequence;
    }

    public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
        return before.Concat(after).ToAsyncEnumerable();
    }

    public IAsyncEnumerable<T> SkipTake(int skip, int take) {
        return this;
    }

    public int Count() => 0;

    public IAsyncEnumerable<E> Select<E>(Func<T, E> selector) => EmptyOperator<E>.Instance;

    public IAsyncEnumerable<T> Where(Func<T, bool> predicate) => this;

    public IAsyncEnumerable<G> AsyncSelect<G>(Func<T, CancellationToken, ValueTask<G>> nextSelector) => EmptyOperator<G>.Instance;

    public IAsyncEnumerable<T> AsyncWhere(Func<T, CancellationToken, ValueTask<bool>> predicate) => this;

    public ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default) {
        return new ValueTask<List<T>>([]);
    }

    public ValueTask<HashSet<T>> ToHashSetAsync(CancellationToken cancellationToken = default) {
        return new ValueTask<HashSet<T>>([]);
    }

    public ValueTask<SortedSet<T>> ToSortedSetAsync(CancellationToken cancellationToken = default) {
        return new ValueTask<SortedSet<T>>([]);
    }

    public IOrderedAsyncEnumerable<T> OrderBy(IComparer<T> comparer) {
        return this;
    }
}