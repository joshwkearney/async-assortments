using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncLinq.Operators {
    internal class FlattenEnumerablesOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<IEnumerable<T>> parent;

        public AsyncOperatorParams Params { get; }

        public FlattenEnumerablesOperator(AsyncOperatorParams pars, IAsyncEnumerable<IEnumerable<T>> parent) {
            this.parent = parent;
            Params = pars;
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
