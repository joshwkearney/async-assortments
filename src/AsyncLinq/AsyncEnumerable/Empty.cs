using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>
    /// Return an empty sequence.
    /// </summary>
    public static IAsyncEnumerable<TSource> Empty<TSource>() => EmptyOperator<TSource>.Instance;
}