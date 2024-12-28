using AsyncAssortments.Operators;

namespace AsyncAssortments.Linq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Performs a subsequent ordering of the elements of a sequence in ascending order using the provided comparer
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="comparer">The comparer that defines the ordering</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IOrderedAsyncEnumerable<TSource> Then<TSource>(
        this IOrderedAsyncEnumerable<TSource> source,
        IComparer<TSource> comparer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        var totalComparer = Comparer<TSource>.Create((x, y) => {
            // Do this branchless so sorting will be faster
            var comp1 = source.Comparer.Compare(x, y);
            var comp2 = comparer.Compare(x, y);

            return comp1 == 0 ? comp2 : comp1;
        });

        return source.Source.Order(totalComparer);
    }

    /// <summary>
    ///     Performs a subsequent ordering of the elements of a sequence in descending order using the provided comparer
    /// </summary>
    /// <param name="source">The source sequence</param>
    /// <param name="comparer">The comparer that defines the ordering</param>
    /// <typeparam name="TSource">The type of the source sequence</typeparam>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    public static IOrderedAsyncEnumerable<TSource> ThenDescending<TSource>(
        this IOrderedAsyncEnumerable<TSource> source,
        IComparer<TSource> comparer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (comparer == null) {
            throw new ArgumentNullException(nameof(comparer));
        }

        var thenComparer = Comparer<TSource>.Create((x, y) => comparer.Compare(y, x));

        return source.Then(thenComparer);
    }
}