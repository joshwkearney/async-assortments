namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    public static async ValueTask<TSource?> FirstOrDefaultAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        await using var enumerator = source.GetAsyncEnumerator(cancellationToken);
        var worked = await enumerator.MoveNextAsync();

        if (!worked) {
            return default;
        }

        return enumerator.Current;
    }
}