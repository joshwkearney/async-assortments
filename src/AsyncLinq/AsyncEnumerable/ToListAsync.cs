namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static ValueTask<List<TSource>> ToListAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return Helper();

        async ValueTask<List<TSource>> Helper() {
            var list = new List<TSource>();

            await foreach (var item in source.WithCancellation(cancellationToken)) {
                list.Add(item);
            }

            return list;
        }
    }
}