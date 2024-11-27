using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncAssortments {
    public interface IOrderedAsyncEnumerable<T> : IAsyncEnumerable<T> {
        public IAsyncEnumerable<T> Source { get; }

        public IComparer<T> Comparer { get; }
    }
}
