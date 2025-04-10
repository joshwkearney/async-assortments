using AsyncAssortments.Operators;
using System.Threading;
using System.Threading.Channels;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Reads an <see cref="IAsyncEnumerable{T}" /> into a channel, and returns
    ///     the channel's reader that can access the sequences elements as they are
    ///     produced. 
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="maxBuffer">
    ///     The maximum size of the channel, If a negative number is supplied, the
    ///     channel is unbounded in size.
    /// </param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used to cancel the enumeration before it finishes.
    /// </param>
    /// <returns>
    ///     A channel reader that can be used to access the sequence's elements
    /// </returns>
    /// <remarks>
    ///     The caller must be sure to read the channel to completion, otherwise
    ///     any exceptions produced by the original sequence will not be handled
    ///     correctly.
    /// </remarks>

    public static Channel<TSource> ToChannel<TSource>(
        this IAsyncEnumerable<TSource> source,
        int maxBuffer = -1,
        CancellationToken cancellationToken = default) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        Channel<TSource> channel;

        if (maxBuffer <= 0) {
            channel = Channel.CreateUnbounded<TSource>(new UnboundedChannelOptions() {
                AllowSynchronousContinuations = false
            });
        }
        else {
            channel = Channel.CreateBounded<TSource>(new BoundedChannelOptions(maxBuffer) {
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.Wait
            });
        }

        // Fire and forget, exceptions will be handled through the channel
        ToChannelHelper(source, channel, cancellationToken);

        return channel;
    }

    private static async void ToChannelHelper<T>(
        IAsyncEnumerable<T> source, 
        Channel<T> channel,
        CancellationToken cancellationToken) {

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