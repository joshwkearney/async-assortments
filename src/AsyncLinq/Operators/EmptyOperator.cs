namespace AsyncLinq.Operators;

internal class EmptyOperator<T> : IAsyncOperator<T>, ISelectWhereTaskOperator<T>, IConcatOperator<T>, 
    IConcatEnumerablesOperator<T>, ISelectWhereOperator<T>, ISkipTakeOperator<T>, ICountOperator<T> {

    public static IAsyncOperator<T> Instance { get; } = new EmptyOperator<T>();
    
    public AsyncOperatorParams Params => default;

    private EmptyOperator() { }

    public IAsyncOperator<T> WithParams(AsyncOperatorParams pars) {
        return new WrapperOperator<T>(pars, this);
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
        yield break;
    }

    public IAsyncEnumerable<G> SelectWhereTask<G>(AsyncSelectWhereFunc<T, G> nextSelector) {
        return EmptyOperator<G>.Instance;
    }

    public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence) {
        return sequence;
    }

    public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
        return before.Concat(after).AsAsyncEnumerable();
    }

    public IAsyncEnumerable<E> SelectWhere<E>(SelectWhereFunc<T, E> nextSelector) {
        return EmptyOperator<E>.Instance;
    }

    public IAsyncEnumerable<T> SkipTake(int skip, int take) {
        return this;
    }

    public int Count() => 0;
}