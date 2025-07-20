namespace AsyncAssortments.Operators {
    internal class FlattenEnumerablesOperator<T> : IAsyncOperator<T>, IConcatOperator<T>, IConcatEnumerablesOperator<T> {
        private readonly IAsyncEnumerable<IEnumerable<T>> parent;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public int MaxConcurrency { get; }

        public FlattenEnumerablesOperator(AsyncEnumerableScheduleMode pars, int maxConcurrency, IAsyncEnumerable<IEnumerable<T>> parent) {
            this.parent = parent;
            this.ScheduleMode = pars;
            this.MaxConcurrency = maxConcurrency;
        }
        
        public IAsyncOperator<T> WithScheduleMode(AsyncEnumerableScheduleMode pars, int maxConcurrency) {
            return new FlattenEnumerablesOperator<T>(pars, maxConcurrency, parent);
        }

        public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence) {
            if (this.parent is WrapEnumerableOperator<IEnumerable<T>> op) {
                var newItems = op.Items.Select(x => x.ToAsyncEnumerable()).Append(sequence);
                var newParent = new WrapEnumerableOperator<IAsyncEnumerable<T>>(op.ScheduleMode, op.MaxConcurrency, newItems);
                
                return new FlattenOperator<T>(this.ScheduleMode, this.MaxConcurrency, newParent);
            }
            else {
                return new FlattenOperator<T>(this.ScheduleMode, this.MaxConcurrency, new[] { this, sequence }.ToAsyncEnumerable());
            }
        }

        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
            if (this.parent is WrapEnumerableOperator<IEnumerable<T>> op) {
                var newItems = op.Items.Prepend(before).Append(after);
                var newParent = new WrapEnumerableOperator<IEnumerable<T>>(op.ScheduleMode, op.MaxConcurrency,newItems);
                
                return new FlattenEnumerablesOperator<T>(this.ScheduleMode, this.MaxConcurrency, newParent);
            }
            else {
                return new ConcatEnumerablesOperator<T>(this.ScheduleMode, this.MaxConcurrency, this, before, after);
            }
        }
        
        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in parent.WithCancellation(cancellationToken)) {
                foreach (var sub in item) {
                    yield return sub;
                }
            }
        }
    }
}
