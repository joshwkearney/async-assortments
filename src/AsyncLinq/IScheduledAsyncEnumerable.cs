using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncLinq {
    public interface IScheduledAsyncEnumerable<out T> : IAsyncEnumerable<T> {
        public AsyncEnumerableScheduleMode ScheduleMode { get; }
    }

    public enum AsyncEnumerableScheduleMode {
        Sequential,
        ConcurrentOrdered,
        ConcurrentUnordered,
        ParallelOrdered,
        ParallelUnordered
    }

    internal static class AsyncPipelineExecutionExtensions {
        internal static AsyncEnumerableScheduleMode GetPipelineExecution<T>(this IAsyncEnumerable<T> source) {
            var execution = AsyncEnumerableScheduleMode.Sequential;

            if (source is IScheduledAsyncEnumerable<T> pipeline) {
                execution = pipeline.ScheduleMode;
            }

            return execution;
        }

        internal static bool IsOrdered(this AsyncEnumerableScheduleMode pars) {
            return pars == AsyncEnumerableScheduleMode.Sequential
                || pars == AsyncEnumerableScheduleMode.ConcurrentOrdered
                || pars == AsyncEnumerableScheduleMode.ParallelOrdered;
        }

        internal static bool IsUnordered(this AsyncEnumerableScheduleMode pars) {
            return pars == AsyncEnumerableScheduleMode.ConcurrentUnordered
                || pars == AsyncEnumerableScheduleMode.ParallelUnordered;
        }

        internal static bool IsParallel(this AsyncEnumerableScheduleMode pars) {
            return pars == AsyncEnumerableScheduleMode.ParallelOrdered
                || pars == AsyncEnumerableScheduleMode.ParallelUnordered;
        }

        internal static AsyncEnumerableScheduleMode MakeConcurrent(this AsyncEnumerableScheduleMode pars) {
            if (pars.IsUnordered()) {
                return AsyncEnumerableScheduleMode.ConcurrentUnordered;
            }
            else {
                return AsyncEnumerableScheduleMode.ConcurrentOrdered;
            }
        }

        internal static AsyncEnumerableScheduleMode MakeParallel(this AsyncEnumerableScheduleMode pars) {
            if (pars.IsUnordered()) {
                return AsyncEnumerableScheduleMode.ParallelUnordered;
            }
            else {
                return AsyncEnumerableScheduleMode.ParallelOrdered;
            }
        }
    }
}
