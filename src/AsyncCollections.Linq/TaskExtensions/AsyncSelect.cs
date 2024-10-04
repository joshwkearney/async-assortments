namespace CollectionTesting {
    public static partial class TaskExtensions {
        public static async Task<E> AsyncSelect<T, E>(this Task<T> task, Func<T, Task<E>> selector) {
            return await selector(await task);
        }

        public static async Task AsyncSelect(this Task task, Func<Task> selector) {
            await task;
            await selector();
        }

        public static async Task<T> AsyncSelect<T>(this Task task, Func<Task<T>> selector) {
            await task;
            return await selector();
        }
    }
}
