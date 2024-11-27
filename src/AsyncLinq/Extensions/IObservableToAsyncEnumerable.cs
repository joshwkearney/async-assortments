using System.Threading.Channels;
using AsyncCollections.Linq.Operators;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerableExtensions {
    /// <summary>
    ///     Converts an <see cref="IObservable{T}" /> into an <see cref="IAsyncEnumerable{T}" />
    ///     by buffering the observable's elements.
    /// </summary>
    /// <param name="maxBuffer">
    ///     The maximum number of items to buffer before new items are dropped.
    /// </param>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> whose enumerators subscribe to the observable
    ///     and yield any items that are pushed to the observable's subscribers.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    /// <remarks>
    ///     This method uses a channel to buffer elements pushed by the observable so that the
    ///     next call to <see cref="IAsyncEnumerator{T}.MoveNextAsync" /> can return it. You 
    ///     may set a maximum number of items to store in this channel, after which new items
    ///     are dropped and not returned by the resulting <see cref="IAsyncEnumerable{T}" />.
    /// </remarks>
    public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(
        this IObservable<TSource> source, 
        int maxBuffer = -1) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return new WrapObservableOperator<TSource>(
            AsyncEnumerableScheduleMode.ConcurrentUnordered,
            source, 
            maxBuffer);
    }
}