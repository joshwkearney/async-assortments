using System.Diagnostics;
using System.Reactive.Linq;
using AsyncLinq;

var first = new int[] { 1, 2, 3 };
var second = first.Concat(new int[0]);

Console.WriteLine(first == second);

await Test();
await Test();
await Test();
await Test();
await Test();

async ValueTask Test() {
    var watch = new Stopwatch();
    watch.Restart();

    //IEnumerable<object> list = new[] { 1, 2, 3 };
    //Task.WhenAll(items.Select(async x => await Something()).ToArray()));
    
    var max = await new[] { 1, 2, 3 }
        .AsAsyncEnumerable()
        .AsParallel(preserveOrder: true)
        .Select(x => x * 100)
        .AsyncSelect(async x => { await Task.Delay(x); return x; })
        .FirstAsync();
    
    Console.WriteLine($"Finished {max} in {watch.ElapsedMilliseconds} ms");
}