using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncLinq.Operators {
    internal class FlattenEnumerablesOperator<T> : IAsyncOperator<T>, IConcatOperator<T>, IConcatEnumerablesOperator<T> {
        private readonly IAsyncEnumerable<IEnumerable<T>> parent;

        public AsyncOperatorParams Params { get; }

        public FlattenEnumerablesOperator(AsyncOperatorParams pars, IAsyncEnumerable<IEnumerable<T>> parent) {
            this.parent = parent;
            Params = pars;
        }
        
        public IAsyncOperator<T> WithParams(AsyncOperatorParams pars) {
            return new FlattenEnumerablesOperator<T>(pars, parent);
        }

        public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence) {
            if (this.parent is EnumerableOperator<IEnumerable<T>> op) {
                var newItems = op.Items.Select(x => x.AsAsyncEnumerable()).Append(sequence);
                var newParent = new EnumerableOperator<IAsyncEnumerable<T>>(op.Params, newItems);
                
                return new FlattenOperator<T>(this.Params, newParent);
            }
            else {
                return new FlattenOperator<T>(this.Params, new[] { this, sequence }.AsAsyncEnumerable());
            }
        }

        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
            if (this.parent is EnumerableOperator<IEnumerable<T>> op) {
                var newItems = op.Items.Prepend(before).Append(after);
                var newParent = new EnumerableOperator<IEnumerable<T>>(op.Params, newItems);
                
                return new FlattenEnumerablesOperator<T>(this.Params, newParent);
            }
            else {
                return new ConcatEnumerablesOperator<T>(this.Params, this, before, after);
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
