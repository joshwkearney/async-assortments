﻿using AsyncLinq.Operators;

namespace AsyncLinq;

public static partial class AsyncEnumerable {
    /// <summary>
    ///     Merges this sequence with another
    /// </summary>
    /// <param name="second">The other sequence to merge</param>
    /// <returns>A combined sequence that enumerates all of the elements in both sequences</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    /// <remarks>
    ///     <para>
    ///         This is an async operator and can run sequentially, concurrently, or in parallel,
    ///         depending on the execution mode of the input sequence. See the <c>.AsSequential()</c>,
    ///         <c>.AsConcurrent()</c>, and <c>.AsParallel()</c> operators for details
    ///     </para>
    ///     <para>
    ///         When run sequentially, <c>Concat</c> enumerates the elements from the first sequence,
    ///         and then the elements from the second sequence. When run concurrently or in parallel
    ///         when order is preserved [<c>.AsConcurrent(true)</c> or <c>.AsParallel(true)</c>], the
    ///         sequences are enumerated at the same time but the results are yielded in their original
    ///         order. When run concurrently or in parallel when order is not preserved 
    ///         [<c>.AsConcurrent(false)</c> or <c>.AsParallel(false)</c>], the sequences are enumerated
    ///         at the same time and the results are yielded as soon they are available.
    ///     </para>
    /// </remarks>
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    public static IAsyncEnumerable<TSource> Concat<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IAsyncEnumerable<TSource> second) {
        
        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }

        if (source == EmptyOperator<TSource>.Instance) {
            return second;
        }

        if (second == EmptyOperator<TSource>.Instance) {
            return source;
        }

        if (second is EnumerableOperator<TSource> listOp) {
            return source.Concat(listOp.Items);
        }

        if (source is IConcatOperator<TSource> concatOp) {
            return concatOp.Concat(second);
        }

        var pars = source.GetPipelineExecution();

        return new FlattenOperator<TSource>(pars, new[] { source, second }.ToAsyncEnumerable());
    }

    /// <summary>
    ///     Merges this sequence with an <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <param name="second">The other sequence to merge</param>
    /// <returns>A combined sequence that enumerates all of the elements in both sequences</returns>
    /// <exception cref="ArgumentNullException">A provided argument was null</exception>
    /// <remarks>
    ///     <para>
    ///         This is an async operator and can run sequentially, concurrently, or in parallel,
    ///         depending on the execution mode of the input sequence. See the <c>.AsSequential()</c>,
    ///         <c>.AsConcurrent()</c>, and <c>.AsParallel()</c> operators for details
    ///     </para>
    ///     <para>
    ///         By default when concatenating an <see cref="IEnumerable{T}"/>, <c>Concat</c> preserves the order
    ///         of the elements, enumerating the source sequence and then the <see cref="IEnumerable{T}"/>.
    ///         However, if the source sequence has an execution mode that does not preserve order 
    ///         [<c>.AsConcurrent(false)</c> or <c>.AsParallel(false)</c>], the elements from the
    ///         <see cref="IEnumerable{T}"/> may be yielded before the original sequence is fully enumerated.
    ///     </para>
    /// </remarks>
    /// <seealso cref="AsSequential{TSource}" />
    /// <seealso cref="AsConcurrent{TSource}" />
    /// <seealso cref="AsParallel{TSource}" />
    public static IAsyncEnumerable<TSource> Concat<TSource>(
        this IAsyncEnumerable<TSource> source, 
        IEnumerable<TSource> second) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (second == null) {
            throw new ArgumentNullException(nameof(second));
        }

        var isSecondEmpty = object.ReferenceEquals(second, Array.Empty<TSource>()) ||
                            object.ReferenceEquals(second, Enumerable.Empty<TSource>());

        if (isSecondEmpty) {
            return source;
        }

        if (source is IConcatEnumerablesOperator<TSource> concatOp) {
            return concatOp.ConcatEnumerables([], second);
        }

        var pars = source.GetPipelineExecution();

        return new ConcatEnumerablesOperator<TSource>(pars, source, [], second);
    }
}