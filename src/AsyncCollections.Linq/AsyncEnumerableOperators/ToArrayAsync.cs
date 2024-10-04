using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static ValueTask<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> sequence) {
        if (sequence is IAsyncEnumerableOperator<T> collection && collection.Count > 0) {
            return CollectionToArray(collection);
        }

        return ToArrayAsyncHelper(sequence);
    }

    private static async ValueTask<T[]> ToArrayAsyncHelper<T>(IAsyncEnumerable<T> sequence) {
        var list = await sequence.ToListAsync();

        return list.ToArray();
    }

    private static async ValueTask<T[]> CollectionToArray<T>(IAsyncEnumerableOperator<T> collection) {
        var result = new T[collection.Count];
        var i = 0;

        await foreach (var item in collection) {
            result[i] = item;
            i++;
        }

        return result;
    }
}