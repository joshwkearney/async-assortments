using System.Collections;

namespace AsyncCollections.Linq {
    internal struct ErrorCollection : IReadOnlyCollection<Exception> {
        private List<Exception>? errors;

        public int Count => errors?.Count ?? 0;

        public Exception this[int index] {
            get {
                if (index < 0) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (this.errors == null) {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return this.errors[index];
            }
        }

        public void Add(Exception ex) {
            errors ??= [];

            if (errors.Contains(ex)) {
                return;
            }

            if (ex is AggregateException ag) {
                errors.AddRange(ag.InnerExceptions);
            }
            else {
                errors.Add(ex);
            }
        }

        public Exception? ToException() {
            if (this.errors == null || this.errors.Count == 0) {
                return null;
            }

            if (this.errors.Count == 1) {
                return this.errors[0];
            }
            else {
                return new AggregateException(this.errors);
            }
        }

        public IEnumerator<Exception> GetEnumerator() { 
            if (this.errors == null) {
                yield break;
            }

            foreach (var item in this.errors) {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
