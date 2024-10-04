using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static async ValueTask<int> CountAsync<T>(this IAsyncEnumerable<T> sequence) {
        if (sequence is IAsyncEnumerableOperator<T> collection && collection.Count > 0) {
            return collection.Count;
        }

        int count = 0;

        await foreach (var _ in sequence) {
            count++;
        }

        return count;
    }
}