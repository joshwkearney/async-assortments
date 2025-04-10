using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Produces a set difference between two sequences.
    /// </summary>
    /// <remarks>
    ///     <para>Uses the provided equality comparer to compare elements.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <param name="comparer">The equality comparer used to compare elements</param>
    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        return source.ExceptBy(second, x => x, comparer);
    }

    /// <summary>
    ///     Produces a set difference between two sequences.
    /// </summary>
    /// <remarks>
    ///     <para>Uses the default equality comparer to compare elements.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        IAsyncEnumerable<TSource> second) {

        return source.Except(second, EqualityComparer<TSource>.Default);
    }

    /// <summary>
    ///     Produces a set differences between two sequences.
    /// </summary>
    /// <remarks>
    ///     <para>Uses the provided equality comparer to compare elements.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <param name="comparer">The equality comparer used to compare elements.</param>
    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Except(second.ToAsyncEnumerable(), comparer);
    }

    /// <summary>
    ///     Produces a set difference between two sequences.
    /// </summary>
    /// <remarks>
    ///     <para>Uses the default equality comparer to compare elements.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        IEnumerable<TSource> second) {

        return source.Except(second.ToAsyncEnumerable(), EqualityComparer<TSource>.Default);
    }

    /// <summary>
    ///     Produces a set differences between two sequences.
    /// </summary>
    /// <remarks>
    ///     <para>Uses the provided equality comparer to compare elements.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    /// <param name="comparer">The equality comparer used to compare elements.</param>
    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second,
        IEqualityComparer<TSource> comparer) {

        return source.Except(second.ToAsyncEnumerable(), comparer);
    }

    /// <summary>
    ///     Produces a set difference between two sequences.
    /// </summary>
    /// <remarks>
    ///     <para>Uses the default equality comparer to compare elements.</para>
    ///     <para>This is a set operation. The resulting sequence will not preserve the order of its elements.</para>
    /// </remarks>
    /// <param name="source">The source sequence.</param>
    /// <param name="second">The second sequence.</param>
    public static IAsyncEnumerable<TSource> Except<TSource>(
        this IAsyncEnumerable<TSource> source,
        IObservable<TSource> second) {

        return source.Except(second.ToAsyncEnumerable(), EqualityComparer<TSource>.Default);
    }
}