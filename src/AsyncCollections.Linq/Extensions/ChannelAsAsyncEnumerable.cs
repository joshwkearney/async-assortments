using System.Threading.Channels;

namespace AsyncCollections.Linq;

public static partial class AsyncCollectionsExtensions {
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this ChannelReader<T> channel) {
        while (true) {
            var canRead = await channel.WaitToReadAsync();

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