using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerableExtensions {
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(
        this ChannelReader<T> channel, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {

        if (channel == null) {
            throw new ArgumentNullException(nameof(channel));
        }

        while (true) {
            var canRead = await channel.WaitToReadAsync(cancellationToken);

            if (!canRead) {
                yield break;
            }

            if (!channel.TryRead(out var item)) {
                yield break;
            }

            yield return item;
        }
    }
}