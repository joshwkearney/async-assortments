using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static ValueTaskAwaiter<IEnumerable<T>> GetAwaiter<T>(this IAsyncEnumerable<T> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return GetAwaiterHelper(source).GetAwaiter();
    }

    public static async ValueTask<IEnumerable<T>> GetAwaiterHelper<T>(this IAsyncEnumerable<T> source) {
        return await source.ToListAsync();
    }
}