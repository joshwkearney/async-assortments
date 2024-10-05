using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static async ValueTask<bool> AnyAsync<T>(this IAsyncEnumerable<T> sequence) {
        return await sequence.GetAsyncEnumerator().MoveNextAsync();
    }
}