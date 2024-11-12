using System.Diagnostics;
using System.Reactive.Linq;
using AsyncLinq;

await Test();
//await Test();
//await Test();
//await Test();
//await Test();

async ValueTask Test() {
    var watch = new Stopwatch();
    watch.Restart();

    //var max = await new int[] { 3, 2, 1 }
    //    .AsAsyncEnumerable()
    //    //.AsConcurrent()
    //    .Select(x => x * 100)
    //    .AsyncSelect(async x => { await Task.Delay(x); return x; })
    //    .AsObservable()
    //    //.AsyncSelect(async x => { Thread.Sleep(x); return x; })
    //    .FirstAsync();

    //Console.WriteLine($"Finished {max} in {watch.ElapsedMilliseconds} ms");

    var first = new Dictionary<int, int>() {
        [0] = 1,
        [1] = 2
    };

    var second = new Dictionary<int, int>() {
        [0] = 100,
        [1] = 200
    };

    //var result = from x in first.AsAsyncEnumerable()
    //             join y in second.AsAsyncEnumerable() on x.Key % 2 equals y.Key % 2 into z
    //             select z.Sum();

    var result2 = first
        .AsAsyncEnumerable()
        .AsConcurrent()
        .Join(second.AsAsyncEnumerable(), x => 0, x => 0, (x, y) => (x.Value, y.Value));

    await foreach (var (x, y) in result2) {
        Console.WriteLine($"({x}, {y})");
    }
}