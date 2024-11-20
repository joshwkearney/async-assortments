namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static ValueTask<HashSet<TSource>> ToHashSetAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return ToHashSetHelper(source, cancellationToken);
    }

    private static async ValueTask<HashSet<TSource>> ToHashSetHelper<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken) {

        var list = new HashSet<TSource>();

        await foreach (var item in source.WithCancellation(cancellationToken)) {
            list.Add(item);
        }

        return list;
    }
}