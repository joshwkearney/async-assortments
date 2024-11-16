using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>Appends a value to the end of the sequence.</summary>
    /// <param name="elementProducer">An async function that returns the element to append.</param>
    /// <returns>A new sequence that ends with the new element.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> AsyncAppend<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<ValueTask<TSource>> elementProducer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (elementProducer == null) {
            throw new ArgumentNullException(nameof(elementProducer));
        }

        return source.Concat(elementProducer().AsAsyncEnumerable());
    }
}