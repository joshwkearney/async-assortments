using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static async ValueTask<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> sequence) {
        var enumerator = sequence.GetAsyncEnumerator();
        var worked = await enumerator.MoveNextAsync();

        if (!worked) {
            return default;
        }

        return enumerator.Current;
    }
}