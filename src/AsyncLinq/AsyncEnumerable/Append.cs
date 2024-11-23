﻿using AsyncCollections.Linq.Operators;

namespace AsyncCollections.Linq;

public static partial class AsyncEnumerable {
    /// <summary>Appends a value to the end of the sequence.</summary>
    /// <param name="element">The value to append to the source sequence.</param>
    /// <exception cref="ArgumentNullException">A provided argument was null.</exception>
    public static IAsyncEnumerable<TSource> Append<TSource>(
        this IAsyncEnumerable<TSource> source, 
        TSource element) {

        if (source == null) {
            throw new ArgumentNullException(nameof(source));
        }

        if (source is IConcatEnumerablesOperator<TSource> concatOp) {
            return concatOp.ConcatEnumerables([], [element]);
        }

        var pars = source.GetPipelineExecution();

        return new ConcatEnumerablesOperator<TSource>(pars, source, [], [element]);
    }
}