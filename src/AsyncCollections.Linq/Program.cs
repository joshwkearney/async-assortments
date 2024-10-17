using AsyncCollections.Linq;
using System.Diagnostics;

await Test();
await Test();
await Test();
await Test();
await Test();

async ValueTask Test() {
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