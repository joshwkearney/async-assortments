using System;
using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Produces a sequence composed of the corresponding elements of two sequences.
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TSecond">The type of the second sequence</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<(TSource First, TSecond Second)> Zip<TSource, TSecond>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSecond> second) {
        
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }

        var pars = source.GetScheduleMode();
        
        return new ZipOperator<TSource, TSecond>(pars, source, second);
    }
    
    /// <summary>
    ///     Produces a sequence by applying a selector function to the corresponding elements of two sequences
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="second">The second sequence</param>
    /// <param name="resultSelector">The selector function to apply to elements of both sequences</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <typeparam name="TSecond">The type of the second sequence</typeparam>
    /// <typeparam name="TResult">The type of the resulting sequence</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IAsyncEnumerable<TResult> Zip<TSource, TSecond, TResult>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSecond> second, 
        Func<TSource, TSecond, TResult> resultSelector) {
        
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }
        
        if (resultSelector == null) {
            throw new ArgumentNullException(nameof(resultSelector));
        }

        var pars = source.GetScheduleMode();
        
        return new ZipOperator<TSource, TSecond>(pars, source, second).Select(pair => resultSelector(pair.first, pair.second));
    }
    
    /// <inheritdoc cref="Zip{TSource,TSecond}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSecond})" />
    public static IAsyncEnumerable<(TSource First, TSecond Second)> Zip<TSource, TSecond>(
        this IAsyncEnumerable<TSource> source, 
        IEnumerable<TSecond> second) {
        
        return source.Zip(second.ToAsyncEnumerable());
    }
    
    /// <inheritdoc cref="Zip{TSource,TSecond,TResult}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSecond},Func{TSource,TSecond,TResult})" />
    public static IAsyncEnumerable<TResult> Zip<TSource, TSecond, TResult>(
        this IAsyncEnumerable<TSource> source, 
        IEnumerable<TSecond> second, 
        Func<TSource, TSecond, TResult> resultSelector) {

        return source.Zip(second.ToAsyncEnumerable(), resultSelector);
    }
    
    /// <inheritdoc cref="Zip{TSource,TSecond}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSecond})" />
    public static IAsyncEnumerable<(TSource First, TSecond Second)> Zip<TSource, TSecond>(
        this IAsyncEnumerable<TSource> source, 
        IObservable<TSecond> second) {

        return source.Zip(second.ToAsyncEnumerable());
    }
    
    /// <inheritdoc cref="Zip{TSource,TSecond,TResult}(IAsyncEnumerable{TSource},IAsyncEnumerable{TSecond},Func{TSource,TSecond,TResult})" />
    public static IAsyncEnumerable<TResult> Zip<TSource, TSecond, TResult>(
        this IAsyncEnumerable<TSource> source, 
        IObservable<TSecond> second, 
        Func<TSource, TSecond, TResult> resultSelector) {

        return source.Zip(second.ToAsyncEnumerable(), resultSelector);
    }
}