using AsyncAssortments.Operators;
using System.Threading;
using System.Threading.Channels;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {

    internal static ChannelReader<TSource> ToChannel<TSource>(
        this IAsyncEnumerable<TSource> source,
        int maxBuffer = -1,
        CancellationToken cancellationToken = default) {
        
        Channel<TSource> channel;

        if (maxBuffer <= 0) {
            channel = Channel.CreateUnbounded<TSource>(new UnboundedChannelOptions() {
                AllowSynchronousContinuations = true
            });
        }
        else {
            channel = Channel.CreateBounded<TSource>(new BoundedChannelOptions(maxBuffer) {
                AllowSynchronousContinuations = true,
                FullMode = BoundedChannelFullMode.DropWrite
            });
        }
        
        Iterate();

        return channel.Reader;

        async void Iterate() {
            try {
                await foreach (var item in source.WithCancellation(cancellationToken)) {
                    await channel.Writer.WriteAsync(item);
                }

                channel.Writer.Complete();
            }
            catch (Exception ex) {
                channel.Writer.Complete(ex);
            }
        }
    }
}