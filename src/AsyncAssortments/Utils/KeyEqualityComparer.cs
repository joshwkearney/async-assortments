using System;

namespace AsyncAssortments {
    internal class KeyEqualityComparer<T, TKey> : IEqualityComparer<T> {
        private readonly Func<T, TKey> keySelector;
        private readonly IEqualityComparer<TKey> comparer;

        public KeyEqualityComparer(Func<T, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            this.keySelector = keySelector;
            this.comparer = comparer;
        }

        public bool Equals(T? x, T? y) {
            if (x == null && y == null) {
                return true;
            }
            else if (x == null || y == null) {
                return false;
            }
            else {
                return this.comparer.Equals(this.keySelector(x), this.keySelector(y));
            }
        }

        public int GetHashCode(T obj) {
            var key = this.keySelector(obj);
            
            return key == null ? 0 : this.comparer.GetHashCode(key);
        }
    }
}

