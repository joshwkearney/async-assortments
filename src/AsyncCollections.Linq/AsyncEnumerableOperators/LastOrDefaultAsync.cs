using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static async ValueTask<T?> LastOrDefaultAsync<T>(this IAsyncEnumerable<T> sequence) {
        var last = default(T);

        await foreach (var item in sequence) {
            last = item;
        }

        return last;
    }
}