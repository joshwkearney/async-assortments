namespace AsyncCollections.Linq.Operators;

internal interface IScheduledAsyncOperator<out T> : IScheduledAsyncEnumerable<T> {

    public IScheduledAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode pars);
}

internal interface ICountOperator<T> : IScheduledAsyncOperator<T> {
    public int Count();
}

internal interface IConcatOperator<T> : IScheduledAsyncOperator<T> {
    public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence);
}

internal interface IConcatEnumerablesOperator<T> : IScheduledAsyncOperator<T> {
    public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after);
}

internal interface ISelectOperator<T> : IScheduledAsyncOperator<T> {
    public IAsyncEnumerable<E> Select<E>(Func<T, E> selector);
}

internal interface IWhereOperator<T> : IScheduledAsyncOperator<T> {
    public IAsyncEnumerable<T> Where(Func<T, bool> predicate);
}

internal interface IScheduledAsyncSelectOperator<E> : IScheduledAsyncOperator<E> {
    public IAsyncEnumerable<G> AsyncSelect<G>(Func<E, CancellationToken, ValueTask<G>> nextSelector);
}

internal interface IScheduledAsyncWhereOperator<E> : IScheduledAsyncOperator<E> {
    public IAsyncEnumerable<E> AsyncWhere(Func<E, CancellationToken, ValueTask<bool>> predicate);
}


internal interface ISkipTakeOperator<out T> : IScheduledAsyncOperator<T> {
    public IAsyncEnumerable<T> SkipTake(int skip, int take);
}