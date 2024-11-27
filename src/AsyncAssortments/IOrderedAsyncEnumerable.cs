using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncAssortments {
    public interface IOrderedAsyncEnumerable<T> : IAsyncEnumerable<T> {
        public IComparer<T> Comparer { get; }

        public IAsyncEnumerable<T> Source { get; }
    }
}
