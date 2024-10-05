using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static async ValueTask<HashSet<T>> ToHashSetAsync<T>(this IAsyncEnumerable<T> sequence) {
        var list = new HashSet<T>();

        await foreach (var item in sequence) {
            list.Add(item);
        }

        return list;
    }
}