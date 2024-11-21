namespace AsyncLinq.Operators;

internal class EmptyOperator<T> : IAsyncOperator<T>, IAsyncSelectOperator<T>, IAsyncWhereOperator<T>, 
    IConcatOperator<T>, IConcatEnumerablesOperator<T>, ISelectOperator<T>, IWhereOperator<T>, 
    ISkipTakeOperator<T>, ICountOperator<T> {

    public static IAsyncOperator<T> Instance { get; } = new EmptyOperator<T>();
    
    public AsyncPipelineExecution Execution => default;

    private EmptyOperator() { }

    public IAsyncOperator<T> WithExecution(AsyncPipelineExecution pars) {
        return new WrapperOperator<T>(pars, this);
    }

    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
        yield break;
    }

    public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence) {
        return sequence;
    }

    public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
        return before.Concat(after).AsAsyncEnumerable();
    }

    public IAsyncEnumerable<T> SkipTake(int skip, int take) {
        return this;
    }

    public int Count() => 0;

    public IAsyncEnumerable<E> Select<E>(Func<T, E> selector) => EmptyOperator<E>.Instance;

    public IAsyncEnumerable<T> Where(Func<T, bool> predicate) => this;

    public IAsyncEnumerable<G> AsyncSelect<G>(Func<T, CancellationToken, ValueTask<G>> nextSelector) => EmptyOperator<G>.Instance;

    public IAsyncEnumerable<T> AsyncWhere(Func<T, CancellationToken, ValueTask<bool>> predicate) => this;
}