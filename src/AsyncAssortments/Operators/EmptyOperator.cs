namespace AsyncAssortments.Operators;

internal class EmptyOperator<T> : IAsyncOperator<T>, IAsyncSelectOperator<T>, IAsyncWhereOperator<T>, 
    IConcatOperator<T>, IConcatEnumerablesOperator<T>, ISelectOperator<T>, IWhereOperator<T>, 
    ISkipTakeOperator<T>, ICountOperator<T>, IToListOperator<T>, IToHashSetOperator<T>,
    IOrderOperator<T>, IOrderedAsyncEnumerable<T> {

    public static IAsyncOperator<T> Instance { get; } = new EmptyOperator<T>();
    
    private static IAsyncEnumerator<T> EnumeratorInstance { get; } = new EmptyEnumerator();
    
    public AsyncEnumerableScheduleMode ScheduleMode => default;

    public IAsyncEnumerable<T> Source => this;

    public IComparer<T> Comparer => Comparer<T>.Default;

    public int MaxConcurrency => -1;

    private EmptyOperator() { }

    public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars, int maxConcurrency) {
        return new WrapAsyncEnumerableOperator<T>(pars, maxConcurrency, this);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
        return EnumeratorInstance;
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

    public ValueTask<HashSet<T>> ToHashSetAsync(IEqualityComparer<T> comparer, CancellationToken cancellationToken = default) {
        return new ValueTask<HashSet<T>>(new HashSet<T>(comparer));
    }

    public IOrderedAsyncEnumerable<T> Order(IComparer<T> comparer) {
        return this;
    }

    private class EmptyEnumerator : IAsyncEnumerator<T> {
        public ValueTask DisposeAsync() => new ValueTask();

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(false);

        public T Current => default!;
    }
}