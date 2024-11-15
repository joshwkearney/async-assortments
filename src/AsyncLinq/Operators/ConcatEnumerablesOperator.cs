using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncLinq.Operators {
    internal interface IConcatEnumerablesOperator<T> : IAsyncOperator<T> {
        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after);
    }

    internal class ConcatEnumerablesOperator<T> : IAsyncOperator<T>, IConcatEnumerablesOperator<T>, IConcatOperator<T> {
        private readonly IEnumerable<T> before;
        private readonly IEnumerable<T> after;
        private readonly IAsyncEnumerable<T> parent;

        public AsyncOperatorParams Params { get; }

        public ConcatEnumerablesOperator(
            IAsyncEnumerable<T> parent,
            IEnumerable<T> before, 
            IEnumerable<T> after, 
            AsyncOperatorParams pars) {

            this.parent = parent;
            this.before = before;
            this.after = after;
            Params = pars;
        }

        public IAsyncEnumerable<T> ConcatEnumerables(IEnumerable<T> before, IEnumerable<T> after) {
            return new ConcatEnumerablesOperator<T>(
                this.parent,
                before.Concat(this.before),
                this.after.Concat(after),
                this.Params);
        }
        
        public IAsyncEnumerable<T> Concat(IAsyncEnumerable<T> sequence) {
            var seqs = new[] { this.before.AsAsyncEnumerable(), this.parent, this.after.AsAsyncEnumerable(), sequence };

            return new ConcatOperator<T>(seqs, this.Params);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            if (this.Params.IsUnordered) {
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
