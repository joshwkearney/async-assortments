using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class ChannelExtensions {
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