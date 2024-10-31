using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> AsyncJoin<T, E, TKey, TResult>(
        this IAsyncEnumerable<T> sequence,
        IAsyncEnumerable<E> other,
        Func<T, TKey> keySelector1,
        Func<E, TKey> keySelector2,
        Func<T, E, ValueTask<TResult>> resultSelector) where TKey : notnull {

        return sequence
            .Join(other, keySelector1, keySelector2, resultSelector)
            .AsyncSelect(x => x);
    }
}