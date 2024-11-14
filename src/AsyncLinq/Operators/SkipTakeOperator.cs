using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncLinq.Operators {
    internal interface ISkipTakeOperator<T> {
        public IAsyncEnumerable<T> ComposeWith(int skip, int take);
    }

    internal class SkipTakeOperator<T> : IAsyncOperator<T>, ISkipTakeOperator<T> {
        private readonly IAsyncEnumerable<T> parent;
        private readonly int skip;
        private readonly int take;

        public AsyncOperatorParams Params { get; }

        public SkipTakeOperator(IAsyncEnumerable<T> parent, int skip, int take, AsyncOperatorParams pars) {
            this.parent = parent;
            this.skip = skip;
            this.take = take;
            this.Params = pars;
        }

        public IAsyncEnumerable<T> ComposeWith(int skip, int take) {
            return new SkipTakeOperator<T>(
                this.parent, 
                this.skip + skip, 
                Math.Min(this.take, take), 
                this.Params);
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await using var iterator = this.parent.GetAsyncEnumerator(cancellationToken);

            int skipped = 0;
            int taken = 0;

            while (skipped < this.skip) {
                var hasNext = await iterator.MoveNextAsync();

                if (!hasNext) {
                    yield break;
                }

                skipped++;
            }

            while (taken < this.take) {
                var hasNext = await iterator.MoveNextAsync();

                if (!hasNext) {
                    yield break;
                }

                yield return iterator.Current;
                taken++;
            }
        }
    }
}
