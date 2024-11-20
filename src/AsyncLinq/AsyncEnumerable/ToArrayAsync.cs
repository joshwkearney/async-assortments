namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static ValueTask<TSource[]> ToArrayAsync<TSource>(
        this IAsyncEnumerable<TSource> source, 
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return ToArrayHelper(source, cancellationToken);
    }

    private static async ValueTask<TSource[]> ToArrayHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken) {

        var list = await source.ToListAsync(cancellationToken);

        return list.ToArray();
    }
}