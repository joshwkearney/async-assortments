using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TResult> AsyncSelectMany<Source, TResult>(
        this IAsyncEnumerable<Source> source, 
        Func<Source, ValueTask<IAsyncEnumerable<TResult>>> selector) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (selector == null) {
            throw new ArgumentNullException(nameof(selector));
        }

        return source.AsyncSelect(selector).SelectMany(x => x);
    }
}