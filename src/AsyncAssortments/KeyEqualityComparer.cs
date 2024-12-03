using System;

namespace AsyncAssortments {
    internal class KeyEqualityComparer<T, TKey> : IEqualityComparer<T> {
        private readonly Func<T, TKey> keySelector;
        private readonly IEqualityComparer<TKey> comparer;

        public KeyEqualityComparer(Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            this.keySelector = keySelector;
            this.comparer = comparer;
        }

        public bool Equals(T x, T y) {
            return this.comparer.Equals(this.keySelector(x), this.keySelector(y));
        }

        public int GetHashCode(T obj) {
            return this.comparer.GetHashCode(this.keySelector(obj));
        }
    }
}

