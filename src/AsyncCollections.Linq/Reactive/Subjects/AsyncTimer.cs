//using System.Diagnostics;

//namespace AsyncCollections.Reactive {
//    public sealed class AsyncTimer : IAsyncEnumerableOperator<AsyncTimer.Tick> {
//        private readonly TimeSpan interval;

//        AsyncExecutionMode IAsyncEnumerableOperator<Tick>.ExecutionMode => AsyncExecutionMode.Sequential;

//        public AsyncTimer(TimeSpan interval) {
//            this.interval = interval;
//        }

//        public AsyncTimer(int millis) : this(TimeSpan.FromMilliseconds(millis)) {
//        }

//        public readonly record struct Tick(TimeSpan Elapsed, long Index) { }

//        public async IAsyncEnumerator<Tick> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
//            var watch = new Stopwatch();
//            watch.Restart();

//            long i = 0;
//            yield return new Tick(default, i);

//            while (true) {
//                var nextTickTime = (i + 1) * this.interval;
//                var waitTime = nextTickTime - watch.Elapsed;

//                if (waitTime > default(TimeSpan)) {
//                    await Task.Delay(waitTime, cancellationToken);
//                }

//                yield return new Tick(watch.Elapsed, i);
//                i++;
//            }
//        }
//    }
//}
