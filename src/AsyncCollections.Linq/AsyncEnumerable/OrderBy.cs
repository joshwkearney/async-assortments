using System.Runtime.CompilerServices;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async IAsyncEnumerable<T> Order<T>(
        this IAsyncEnumerable<T> sequence, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        var list = await sequence.ToListAsync(cancellationToken);

        foreach (var item in list.Order()) {
            yield return item;
        }
    }

    public static async IAsyncEnumerable<T> OrderBy<T, E>(
        this IAsyncEnumerable<T> sequence,
        Func<T, E> keySelector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        var list = await sequence.ToListAsync(cancellationToken);

        foreach (var item in list.OrderBy(keySelector)) {
            yield return item;
        }
    }

    public static async IAsyncEnumerable<T> OrderDescending<T>(
        this IAsyncEnumerable<T> sequence,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        var list = await sequence.ToListAsync(cancellationToken);

        foreach (var item in list.OrderDescending()) {
            yield return item;
        }
    }

    public static async IAsyncEnumerable<T> OrderByDescending<T, E>(
        this IAsyncEnumerable<T> sequence,
        Func<T, E> keySelector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        var list = await sequence.ToListAsync(cancellationToken);

        foreach (var item in list.OrderByDescending(keySelector)) {
            yield return item;
        }
    }
}