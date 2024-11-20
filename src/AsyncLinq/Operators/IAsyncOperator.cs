namespace AsyncLinq.Operators;

internal interface IAsyncOperator<out T> : IAsyncPipeline<T> {

    public IAsyncOperator<T> WithExecution(AsyncPipelineExecution pars);
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

internal interface ISelectWhereOperator<T> : IAsyncOperator<T> {
    public IAsyncEnumerable<E> SelectWhere<E>(SelectWhereFunc<T, E> nextSelector);
}

internal interface ISelectWhereTaskOperator<E> : IAsyncOperator<E> {
    public IAsyncEnumerable<G> SelectWhereTask<G>(AsyncSelectWhereFunc<E, G> nextSelector);
}

internal interface ISkipTakeOperator<out T> : IAsyncOperator<T> {
    public IAsyncEnumerable<T> SkipTake(int skip, int take);
}