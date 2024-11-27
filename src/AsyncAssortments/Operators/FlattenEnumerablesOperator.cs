namespace AsyncAssortments.Operators {
    internal class FlattenEnumerablesOperator<T> : IAsyncOperator<T>, IConcatOperator<T>, IConcatEnumerablesOperator<T> {
        private readonly IAsyncEnumerable<IEnumerable<T>> parent;

        public AsyncEnumerableScheduleMode ScheduleMode { get; }

        public FlattenEnumerablesOperator(AsyncEnumerableScheduleMode pars, IAsyncEnumerable<IEnumerable<T>> parent) {
            this.parent = parent;
            ScheduleMode = pars;
        }
        
        public IAsyncOperator<T> WithExecution(AsyncEnumerableScheduleMode pars) {
            return new FlattenEnumerablesOperator<T>(pars, parent);
        }

        public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence) {
            if (this.parent is WrapEnumerableOperator<IEnumerable<T>> op) {
                var newItems = op.Items.Select(x => x.ToAsyncEnumerable()).Append(sequence);
                var newParent = new WrapEnumerableOperator<IAsyncEnumerable<T>>(op.ScheduleMode, newItems);
                
                return new FlattenOperator<T>(this.ScheduleMode, newParent);
            }
            else {
                return new FlattenOperator<T>(this.ScheduleMode, new[] { this, sequence }.ToAsyncEnumerable());
            }
        }

        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
            if (this.parent is WrapEnumerableOperator<IEnumerable<T>> op) {
                var newItems = op.Items.Prepend(before).Append(after);
                var newParent = new WrapEnumerableOperator<IEnumerable<T>>(op.ScheduleMode, newItems);
                
                return new FlattenEnumerablesOperator<T>(this.ScheduleMode, newParent);
            }
            else {
                return new ConcatEnumerablesOperator<T>(this.ScheduleMode, this, before, after);
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
