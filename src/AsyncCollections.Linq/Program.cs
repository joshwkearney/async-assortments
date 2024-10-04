using CollectionTesting;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

await Test();
await Test();
await Test();
await Test();
await Test();

async ValueTask Test() {
    var channel = Channel.CreateUnbounded<int>();
    var items = channel.Reader.AsAsyncEnumerable();

    var watch = new Stopwatch();
    watch.Restart();

    var first = await new int[] { 300, 200, 100 }
        .AsAsyncEnumerable()
        .AsConcurrent()
        .AsyncSelect(async x => { await Task.Delay(x); return x; })
        .FirstAsync();

    var time = watch.ElapsedMilliseconds;

    Console.WriteLine($"Finished {first} in {time} ms");
}