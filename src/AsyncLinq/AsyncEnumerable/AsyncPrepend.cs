using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    public static IAsyncEnumerable<TSource> AsyncPrepend<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<CancellationToken, ValueTask<TSource>> elementProducer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (elementProducer == null) {
            throw new ArgumentNullException(nameof(elementProducer));
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }
        
        return new SingletonOperator<TSource>(pars, elementProducer).Concat(source);
    }
    
    public static IAsyncEnumerable<TSource> AsyncPrepend<TSource>(
        this IAsyncEnumerable<TSource> source, 
        Func<ValueTask<TSource>> elementProducer) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (elementProducer == null) {
            throw new ArgumentNullException(nameof(elementProducer));
        }

        var pars = new AsyncOperatorParams();

        if (source is IAsyncOperator<TSource> op) {
            pars = op.Params;
        }
        
        return new SingletonOperator<TSource>(pars, _ => elementProducer()).Concat(source);
    }
}