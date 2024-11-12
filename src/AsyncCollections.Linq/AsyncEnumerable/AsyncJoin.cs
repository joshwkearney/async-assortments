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

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        if (other == null) {
            throw new ArgumentNullException(nameof(other));
        }

        if (keySelector1 == null) {
            throw new ArgumentNullException(nameof(keySelector1));
        }

        if (keySelector2 == null) {
            throw new ArgumentNullException(nameof(keySelector2));
        }

        if (resultSelector == null) {
            throw new ArgumentNullException(nameof(resultSelector));
        }

        return sequence
            .Join(other, keySelector1, keySelector2, resultSelector)
            .AsyncSelect(x => x);
    }
}