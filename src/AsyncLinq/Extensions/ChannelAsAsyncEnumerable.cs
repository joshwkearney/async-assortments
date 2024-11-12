using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AsyncLinq;

public static partial class AsyncEnumerableExtensions {
    public static async IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(
        this ChannelReader<TSource> source, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        while (true) {
            var canRead = await source.WaitToReadAsync(cancellationToken);

            if (!canRead) {
                yield break;
            }

            if (!source.TryRead(out var item)) {
                yield break;
            }

            yield return item;
        }
    }
}