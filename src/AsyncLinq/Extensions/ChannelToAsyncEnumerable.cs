using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerableExtensions {
    /// <summary>
    ///     Exposes a <see cref="ChannelReader{T}" /> as an <see cref="IAsyncEnumerable{T}" />
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A <see cref="IAsyncEnumerable{T}" /> that will yield items from the channel when 
    ///     <see cref="IAsyncEnumerator{T}.MoveNextAsync" /> is called on one of its iterators.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <remarks>
    ///     The returned <see cref="IAsyncEnumerable{T}" /> does not perform multicasting, so
    ///     items written to the channel will only be yielded by one of the enumerators if 
    ///     multiple enumerators have been created from the same channel.
    /// </remarks>
    public static async IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(
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