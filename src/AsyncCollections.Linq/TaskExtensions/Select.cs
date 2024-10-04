namespace CollectionTesting {
    public static partial class TaskExtensions {
        public static async Task<E> Select<T, E>(this Task<T> task, Func<T, E> selector) {
            return selector(await task);
        }

        public static async Task Select(this Task task, Action selector) {
            await task;
            selector();
        }

        public static async Task<T> Select<T>(this Task task, Func<T> selector) {
            await task;
            return selector();
        }
    }
}
