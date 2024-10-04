using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static async ValueTask<T> FirstAsync<T>(this IAsyncEnumerable<T> sequence) {
        var enumerator = sequence.GetAsyncEnumerator();
        var worked = await enumerator.MoveNextAsync();

        if (!worked) {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        return enumerator.Current;
    }
}