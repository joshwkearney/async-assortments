using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<E> AsyncSelectMany<T, E>(
        this IAsyncEnumerable<T> sequence, 
        Func<T, ValueTask<IAsyncEnumerable<E>>> selector) {

        if (sequence == null) {
            throw new ArgumentNullException(nameof(sequence));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        return sequence.AsyncSelect(selector).SelectMany(x => x);
    }
}