using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static async ValueTask<T[]> ToArrayAsync<T>(this IAsyncEnumerable<T> sequence) {
        var list = await sequence.ToListAsync();

        return list.ToArray();
    }
}