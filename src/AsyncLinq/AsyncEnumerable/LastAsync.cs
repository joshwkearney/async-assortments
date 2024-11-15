namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static async ValueTask<TSource> LastAsync<TSource>(
        this IAsyncEnumerable<TSource> source,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        await using var iterator = source.GetAsyncEnumerator(cancellationToken);

        if (!await iterator.MoveNextAsync()) {
            throw new InvalidOperationException("Sequence contains no elements");
        }

        var last = iterator.Current;

        while (await iterator.MoveNextAsync()) {
            last = iterator.Current;
        }

        return last;
    }
}