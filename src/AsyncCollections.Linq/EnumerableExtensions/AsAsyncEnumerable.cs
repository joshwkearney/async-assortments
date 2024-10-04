using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class EnumerableExtensions {
    public static IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> sequence) {
        return new ReadOnlyAsyncCollection<T>(sequence, AsyncExecutionMode.Sequential);
    }

    private class ReadOnlyAsyncCollection<T> : IAsyncEnumerableOperator<T> {
        private readonly IEnumerable<T> collection;

        public int Count {
            get {
                if (this.collection is IReadOnlyCollection<T> collection) {
                    return collection.Count;
                }
                else if (this.collection is ICollection<T> collection2) {
                    return collection2.Count;
                }
                else {
                    return -1;
                }
            }
        }

        public AsyncExecutionMode ExecutionMode { get; }

        public ReadOnlyAsyncCollection(IEnumerable<T> collection, AsyncExecutionMode mode) {
            this.collection = collection;
            this.ExecutionMode = mode;
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            foreach (var item in this.collection) {
                yield return item;
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}