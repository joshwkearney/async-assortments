namespace AsyncAssortments.Operators;

internal interface IAsyncOperator<out T> : IScheduledAsyncEnumerable<T> {

    public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars);
}

internal interface ICountOperator<T> : IAsyncOperator<T> {
    public int Count();
}

internal interface IConcatOperator<T> : IAsyncOperator<T> {
    public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence);
}

internal interface IConcatEnumerablesOperator<T> : IAsyncOperator<T> {
    public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after);
}

internal interface ISelectOperator<T> : IAsyncOperator<T> {
    public IAsyncEnumerable<E> Select<E>(Func<T, E> selector);
}

internal interface IWhereOperator<T> : IAsyncOperator<T> {
    public IAsyncEnumerable<T> Where(Func<T, bool> predicate);
}

internal interface IAsyncSelectOperator<E> : IAsyncOperator<E> {
    public IAsyncEnumerable<G> AsyncSelect<G>(Func<E, CancellationToken, ValueTask<G>> nextSelector);
}

internal interface IAsyncWhereOperator<E> : IAsyncOperator<E> {
    public IAsyncEnumerable<E> AsyncWhere(Func<E, CancellationToken, ValueTask<bool>> predicate);
}


internal interface ISkipTakeOperator<out T> : IAsyncOperator<T> {
    public IAsyncEnumerable<T> SkipTake(int skip, int take);
}

internal interface IToListOperator<T> : IAsyncOperator<T> {
    public ValueTask<List<T>> ToListAsync(CancellationToken cancellationToken = default);
}

internal interface IToHashSetOperator<T> : IAsyncOperator<T> {
    public ValueTask<HashSet<T>> ToHashSetAsync(IEqualityComparer<T> comparer, CancellationToken cancellationToken = default);
}

internal interface IOrderOperator<T> : IAsyncOperator<T> {
    public IOrderedAsyncEnumerable<T> Order(IComparer<T> comparer);
}

internal interface IDistinctOperator<T> : IAsyncOperator<T> {
    public IAsyncEnumerable<T> Distinct(IEqualityComparer<T> comparer);
}