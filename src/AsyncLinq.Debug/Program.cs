using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using AsyncLinq;

await Test();
await Test();
await Test();
//await Test();
//await Test();

async ValueTask Test() {
    var watch = new Stopwatch();
    watch.Restart();

    var query = new[] { 100, 200 }
        .AsAsyncEnumerable()
        .AsConcurrent()
        .Append(300)
        .Concat([400, 500])
        .Concat(new[] { 600 }.AsAsyncEnumerable().AsConcurrent());

    var x = 78;

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