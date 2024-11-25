using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncLinq.Operators {
    internal class ConcatEnumerablesOperator<T> : IAsyncOperator<T>, IConcatEnumerablesOperator<T>, 
        IConcatOperator<T> {

        private readonly IEnumerable<T> before;
        private readonly IEnumerable<T> after;
        private readonly IAsyncEnumerable<T> parent;

        public AsyncPipelineExecution Execution { get; }

        public ConcatEnumerablesOperator(
            AsyncPipelineExecution pars,
            IAsyncEnumerable<T> parent,
            IEnumerable<T> before, 
            IEnumerable<T> after) {

            this.parent = parent;
            this.before = before;
            this.after = after;
            this.Execution = pars;
        }
        
        public IAsyncOperator<T> WithExecution(AsyncPipelineExecution pars) {
            return new ConcatEnumerablesOperator<T>(pars, this.parent, this.before, this.after);
        }

        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
            return new ConcatEnumerablesOperator<T>(
                this.Execution,
                this.parent,
                before.Concat(this.before),
                this.after.Concat(after));
        }
        
        public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence) {
            var seqs = new[] { this.before.ToAsyncEnumerable(), this.parent, this.after.ToAsyncEnumerable(), sequence };

            return new FlattenOperator<T>(this.Execution, seqs.ToAsyncEnumerable());
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.Execution.IsUnordered()) {
                return this.UnorderedHelper(cancellationToken);
            }
            else {
                return this.SequentialHelper(cancellationToken);
            }
        }

        private async IAsyncEnumerator<T> UnorderedHelper(CancellationToken cancellationToken) {
            // Yield all of the before items
            foreach (var item in this.before) {
                yield return item;
            }

            await using var iterator = this.parent.GetAsyncEnumerator(cancellationToken);
            var nextTask = iterator.MoveNextAsync();

            // Yield as many from the parent synchronously as we can
            while (nextTask.IsCompletedSuccessfully) {
                yield return iterator.Current;
                nextTask = iterator.MoveNextAsync();
            }

            // Yield all of the after items
            foreach (var item in this.after) {
                yield return item;
            }

            // Yield all the rest of the parent asynchronously
            while (await nextTask) {
                yield return iterator.Current;
                nextTask = iterator.MoveNextAsync();
            }
        }

        private async IAsyncEnumerator<T> SequentialHelper(CancellationToken cancellationToken) {
            foreach (var item in this.before) {
                yield return item;
            }

            await foreach (var item in this.parent) {
                yield return item;
            }

            foreach (var item in this.after) {
                yield return item;
            }
        }
    }
}
