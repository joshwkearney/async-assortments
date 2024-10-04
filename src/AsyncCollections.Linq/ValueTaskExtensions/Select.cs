namespace CollectionTesting {
    public static partial class ValueTaskExtensions {
        public static async ValueTask<E> Select<T, E>(this ValueTask<T> task, Func<T, E> selector) {
            return selector(await task);
        }

        public static async ValueTask Select(this ValueTask task, Action selector) {
            await task;
            selector();
        }

        public static async ValueTask<T> Select<T>(this ValueTask task, Func<T> selector) {
            await task;
            return selector();
        }
    }
}
