using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using AsyncLinq;

var errors = new ErrorCollection();

void Test() {
    errors.Add(new InvalidOperationException());
}

Test();

Console.WriteLine(errors.Count);

//await Test();
//await Test();
//await Test();
////await Test();
////await Test();

//async ValueTask Test() {
//    var watch = new Stopwatch();
//    watch.Restart();

//    //IEnumerable<object> list = new[] { 1, 2, 3 };
//    //Task.WhenAll(items.Select(async x => await Something()).ToArray()));

//    var query = new[] { 1500, 100 }
//        .AsAsyncEnumerable()
//        .AsConcurrent(preserveOrder: false)
//        .SelectMany(x => new[] { x, x }.AsAsyncEnumerable().AsConcurrent().AsyncSelect(async y => { await Task.Delay(y); return y; }));

//    await foreach (var item in query) {
//        Console.Write(item + " ");
//    }

//    Console.WriteLine();

//    //Console.WriteLine($"Finished {max} in {watch.ElapsedMilliseconds} ms");
//}