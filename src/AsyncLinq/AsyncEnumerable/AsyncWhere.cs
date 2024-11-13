using System.Runtime.CompilerServices;
using System.Threading;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> AsyncWhere<TSource>(
        this IAsyncEnumerable<TSource> source,
        Func<TSource, ValueTask<bool>> predicate) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (predicate == null) {
            throw new ArgumentNullException(nameof(predicate));
        }

        var sequence = source.AsyncSelect(async x => new AsyncWhereRecord<TSource>(x, await predicate(x)));
        var pars = new AsyncOperatorParams();

        if (sequence is IAsyncOperator<AsyncWhereRecord<TSource>> op) {
            pars = op.Params;
        }

        return new AsyncWhereOperator<TSource>(sequence, pars);
    }

    private class AsyncWhereOperator<T> : IAsyncOperator<T> {
        private readonly IAsyncEnumerable<AsyncWhereRecord<T>> parent;

        public AsyncOperatorParams Params { get; }

        public AsyncWhereOperator(
            IAsyncEnumerable<AsyncWhereRecord<T>> parent, 
            AsyncOperatorParams pars) {

            this.parent = parent;
            this.Params = pars;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            await foreach (var item in this.parent) {
                if (item.IsValid) {
                    yield return item.Value;
                }
            }
        }
    }

    private readonly struct AsyncWhereRecord<T> {
        public readonly T Value;
        public readonly bool IsValid;

        public AsyncWhereRecord(T value, bool isValid) {
            this.Value = value;
            this.IsValid = isValid;
        }
    }
}