using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static async ValueTask<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> sequence) {
        int size = 10;

        if (sequence is IAsyncEnumerableOperator<T> collection && collection.Count > 0) {
            size = collection.Count;
        }

        var list = new List<T>(size);

        await foreach (var item in sequence) {
            list.Add(item);
        }

        return list;
    }
}