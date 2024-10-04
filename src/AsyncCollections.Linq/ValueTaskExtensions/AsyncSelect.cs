namespace CollectionTesting {
    public static partial class ValueTaskExtensions {
        public static async ValueTask<E> AsyncSelect<T, E>(this ValueTask<T> task, Func<T, ValueTask<E>> selector) {
            return await selector(await task);
        }

        public static async ValueTask AsyncSelect(this ValueTask task, Func<ValueTask> selector) {
            await task;
            await selector();
        }

        public static async ValueTask<T> AsyncSelect<T>(this ValueTask task, Func<ValueTask<T>> selector) {
            await task;
            return await selector();
        }
    }
}