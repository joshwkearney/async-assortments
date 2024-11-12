namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static async ValueTask<List<TSource>> ToListAsync<TSource>(
        this IAsyncEnumerable<TSource> source, 
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        var list = new List<TSource>();

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            list.Add(item);
        }

        return list;
    }
}