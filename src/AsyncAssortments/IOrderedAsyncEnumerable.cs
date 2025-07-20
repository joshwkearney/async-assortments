namespace AsyncAssortments {
    /// <summary>
    ///     Represents an <see cref="IAsyncEnumerable{T}" /> that has an ordering applied to it.
    /// </summary>
    /// <inheritdoc cref="IAsyncEnumerable{T}" />
    public interface IOrderedAsyncEnumerable<T> : IAsyncEnumerable<T> {
        /// <summary>
        ///     The original sequence without any ordering applied.
        /// </summary>
        public IAsyncEnumerable<T> Source { get; }

        /// <summary>
        ///     A comparer which defines the ordering applied to the original sequence.
        /// </summary>
        public IComparer<T> Comparer { get; }
    }
}
