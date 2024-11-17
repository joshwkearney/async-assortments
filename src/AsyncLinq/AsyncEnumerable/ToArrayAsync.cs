namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static async ValueTask<TSource[]> ToArrayAsync<TSource>(
        this IAsyncEnumerable<TSource> source, 
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var list = await source.ToListAsync(cancellationToken);

        return list.ToArray();
    }
}