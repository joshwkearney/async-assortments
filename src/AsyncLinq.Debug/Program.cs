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

    var query = new[] { 1, 2, 3, 4, 5, 6 }
        .AsAsyncEnumerable()
        .AsConcurrent(preserveOrder: false)
        .AsyncWhere(async x => x <= 3)
        .Select(x => x * 100)
        .AsyncSelect(async x => {
            await Task.Delay(x);
            return x;
        });

    var max = await query.FirstAsync();
    
    Console.WriteLine($"Finished {max} in {watch.ElapsedMilliseconds} ms");
}