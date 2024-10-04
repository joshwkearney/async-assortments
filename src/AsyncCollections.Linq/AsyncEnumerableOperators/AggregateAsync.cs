using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CollectionTesting;

public static partial class AsyncEnumerableExtensions {
    public static async ValueTask<E> AggregateAsync<T, E>(this IAsyncEnumerable<T> sequence, E seed, Func<E, T, E> reducer) {
        await foreach (var item in sequence) {
            seed = reducer(seed, item);
        }

        return seed;
    }

    public static ValueTask<T> Sum<T>(this IAsyncEnumerable<T> sequence) where T : INumber<T> {
        return sequence.AggregateAsync(T.Zero, (x, y) => x + y);
    }

    public static async ValueTask<float> Sum(this IAsyncEnumerable<float> sequence) {
        return (float)(await sequence.AggregateAsync(0d, (x, y) => x + y));
    }
}