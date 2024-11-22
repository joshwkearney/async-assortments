using System.Collections;
using AsyncLinq.Operators;
using System.Linq;

namespace AsyncLinq;

public static partial class AsyncEnumerableExtensions {
    /// <summary>
    ///     Exposes an <see cref="IEnumerable" /> as an <see cref="IAsyncEnumerable{T}" />.
    /// </summary>
    /// <returns>
    ///     A <see cref="IAsyncEnumerable{T}" /> containing all of the elements in the 
    ///     original sequence.
    /// </returns>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> AsAsyncEnumerable<TSource>(this IEnumerable<TSource> source) {
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        // No need to allocate extra objects if we don't need to
        if (object.ReferenceEquals(source, Array.Empty<TSource>()) || object.ReferenceEquals(source, Enumerable.Empty<TSource>())) {
            return EmptyOperator<TSource>.Instance;
        }

        return new EnumerableOperator<TSource>(default, source);
    }    
}