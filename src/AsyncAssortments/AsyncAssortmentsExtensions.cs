using System.Collections;
using System.Runtime.CompilerServices;
using AsyncAssortments.Linq;
using AsyncAssortments.Operators;

namespace AsyncAssortments;

public static class AsyncAssortmentsExtensions {
    /// <summary>
    ///     Exposes an <see cref="IEnumerable" /> as an <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    /// <returns>
    ///     A <see cref="IAsyncEnumerable{T}" /> containing all of the elements in the 
    ///     original sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this IEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        // No need to allocate extra objects if we don't need to
        if (object.ReferenceEquals(source, Array.Empty<TSource>()) || object.ReferenceEquals(source, Enumerable.Empty<TSource>())) {
            return EmptyOperator<TSource>.Instance;
        }

        return new WrapEnumerableOperator<TSource>(default, source);
    }

    /// <summary>
    ///     Converts an <see cref="IObservable{T}" /> into an <see cref="IAsyncEnumerable{T}" />
    ///     by buffering the observable's elements. The returned observable is scheduled to
    ///     be concurrent without preserving the order of elements 
    ///     (like <c>.AsConcurrent(preserveOrder: false)</c> would return) in order to match 
    ///     the behavior of observables.
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
    ///     are dropped and not yielded by the resulting <see cref="IAsyncEnumerable{T}" />.
    /// </remarks>
    public static IScheduledAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(
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
    
    /// <summary>
    ///     Exposes a <see cref="Task{TResult}" /> as an <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> that will yield the result of awaiting the
    ///     provided task as the only item in the sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this Task<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return new WrapAsyncFuncOperator<TSource>(default, _ => new ValueTask<TSource>(source));
    }
    
    /// <summary>
    ///     Exposes a <see cref="ValueTask{TResult}" /> as an <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    /// <returns>
    ///     An <see cref="IAsyncEnumerable{T}" /> that will yield the result of awaiting the
    ///     provided task as the only item in the sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(this ValueTask<TSource> source) {
        // We need to do this because you can call .GetEnumerator() multiple times on a sequence,
        // which will await our ValueTask multiple times
        return source.AsTask().ToAsyncEnumerable();
    }
    
    public static ValueTaskAwaiter<IEnumerable<T>> GetAwaiter<T>(this IAsyncEnumerable<T> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        return source
            .ToListAsync()
            .Select(x => (IEnumerable<T>)x)
            .GetAwaiter();
    }

    internal static async Task<E> Select<T, E>(this Task<T> task, Func<T, E> selector) {
        return selector(await task);
    }
    
    internal static async Task<E> AsyncSelect<T, E>(this Task<T> task, Func<T, Task<E>> selector) {
        return await selector(await task);
    }
    
    internal static async ValueTask<E> Select<T, E>(this ValueTask<T> task, Func<T, E> selector) {
        return selector(await task);
    }
    
    internal static async ValueTask<E> AsyncSelect<T, E>(this ValueTask<T> task, Func<T, ValueTask<E>> selector) {
        return await selector(await task);
    }
}