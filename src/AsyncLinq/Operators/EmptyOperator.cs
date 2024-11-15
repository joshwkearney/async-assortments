namespace AsyncLinq.Operators;

internal class EmptyOperator<T> : IAsyncOperator<T>, IAsyncSelectWhereOperator<T>, IConcatOperator<T>, 
    IEnumerableConcatOperator<T>, ISelectWhereOperator<T>, ISkipTakeOperator<T> {
    public static IAsyncOperator<T> Instance { get; } = new EmptyOperator<T>();
    
    public AsyncOperatorParams Params => default;

    private EmptyOperator() { }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
        yield break;
    }

    public IAsyncEnumerable<G> AsyncSelectWhere<G>(AsyncSelectWhereFunc<T, G> nextSelector) {
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
}