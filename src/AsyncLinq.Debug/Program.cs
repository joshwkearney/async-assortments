using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using AsyncLinq;

await Test();
await Test();
await Test();
//await Test();
//await Test();

//new[] { 1, 2, 3 }
//    .AsAsyncEnumerable()
//    .AsyncSelectMany(async x => new int[] { x, x });

async ValueTask Test() {
    var watch = new Stopwatch();
    watch.Restart();

    try {
        var query = await new[] { 100 }
            .AsAsyncEnumerable()
            .AsConcurrent(preserveOrder: false)
            .AsyncSelect(async (x, c) => {
                await Task.Delay(200, c);
                return x;
            })
            .Concat(new[] { 200 }.AsAsyncEnumerable().AsyncSelect(async (x, c) => {
                await Task.Delay(500);
                Console.WriteLine("GOTTEM! " + x);
                return x;
            }))
            .FirstAsync();
        
        Console.WriteLine(query);
        Console.WriteLine();
    }
    catch (Exception ex) {
        Console.WriteLine("Cancelled in " + watch.ElapsedMilliseconds + " ms");
    }

    // var query = new[] { 1500, 100 }
    //     .AsAsyncEnumerable()
    //     .AsConcurrent(preserveOrder: false)
    //     .SelectMany(x => new[] { x, x }.AsAsyncEnumerable().AsyncSelect(async y => { await Task.Delay(y); return y; }));
    //
    // await foreach (var item in query) {
    //     Console.Write(item + " ");
    // }
    //
    // Console.WriteLine();

    //Console.WriteLine($"Finished {max} in {watch.ElapsedMilliseconds} ms");
}