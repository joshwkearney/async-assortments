using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncLinq {
    public interface IAsyncPipeline<out T> : IAsyncEnumerable<T> {
        public AsyncPipelineExecution Execution { get; }
    }

    public enum AsyncPipelineExecution {
        Sequential,
        ConcurrentOrdered,
        ConcurrentUnordered,
        ParallelOrdered,
        ParallelUnordered
    }

    internal static class AsyncPipelineExecutionExtensions {
        internal static AsyncPipelineExecution GetPipelineExecution<T>(this IAsyncEnumerable<T> source) {
            var execution = AsyncPipelineExecution.Sequential;

            if (source is IAsyncPipeline<T> pipeline) {
                execution = pipeline.Execution;
            }

            return execution;
        }

        internal static bool IsOrdered(this AsyncPipelineExecution pars) {
            return pars == AsyncPipelineExecution.Sequential
                || pars == AsyncPipelineExecution.ConcurrentOrdered
                || pars == AsyncPipelineExecution.ParallelOrdered;
        }

        internal static bool IsUnordered(this AsyncPipelineExecution pars) {
            return pars == AsyncPipelineExecution.ConcurrentUnordered
                || pars == AsyncPipelineExecution.ParallelUnordered;
        }

        internal static bool IsParallel(this AsyncPipelineExecution pars) {
            return pars == AsyncPipelineExecution.ParallelOrdered
                || pars == AsyncPipelineExecution.ParallelUnordered;
        }

        internal static AsyncPipelineExecution MakeConcurrent(this AsyncPipelineExecution pars) {
            if (pars.IsUnordered()) {
                return AsyncPipelineExecution.ConcurrentUnordered;
            }
            else {
                return AsyncPipelineExecution.ConcurrentOrdered;
            }
        }

        internal static AsyncPipelineExecution MakeParallel(this AsyncPipelineExecution pars) {
            if (pars.IsUnordered()) {
                return AsyncPipelineExecution.ParallelUnordered;
            }
            else {
                return AsyncPipelineExecution.ParallelOrdered;
            }
        }
    }
}
