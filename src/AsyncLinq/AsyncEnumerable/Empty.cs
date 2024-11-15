using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> Empty<TSource>() => EmptyOperator<TSource>.Instance;
}