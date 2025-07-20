using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Returns the distinct elements in a sequence, using the provided <see cref="IEqualityComparer{T}" /> to
    ///     evaluate if two elements are equal.
    /// </summary>
    /// <param name="source">The source sequence.</param>
    /// <param name="comparer">The comparer used to evaluate equality.</param>
    /// <typeparam name="TSource">The type of the source sequence.</typeparam>
    /// <returns>A sequence containing only the distinct elements of the original sequence.</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> Distinct<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEqualityComparer<TSource> comparer) {
        
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        if (source is IDistinctOperator<TSource> op) {
            return op.Distinct(comparer);
        }

        return new DistinctOperator<TSource>(
            source.GetScheduleMode(),
            source.GetMaxConcurrency(),
            source,
            comparer);
    }

    /// <inheritdoc cref="Distinct{TSource}(System.Collections.Generic.IAsyncEnumerable{TSource},System.Collections.Generic.IEqualityComparer{TSource})" />
    /// <summary>
    ///     Returns the distinct elements in a sequence
    /// </summary>
    public static IAsyncEnumerable<TSource> Distinct<TSource>(
        this IAsyncEnumerable<TSource> source) {

        return source.Distinct(EqualityComparer<TSource>.Default);
    }
}