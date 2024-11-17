using System.Diagnostics;

namespace AsyncLinq.Tests;

public class AsParallelTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;

        Assert.Throws<ArgumentNullException>(() => nullSeq.AsParallel(true));
        Assert.Throws<ArgumentNullException>(() => nullSeq.AsParallel(false));
    }
    
    [Fact]
    public async Task TestOrdered1() {
        var watch = new Stopwatch();
        watch.Start();
        
        var items = await new[] { 300, 200, 100 }
            .AsAsyncEnumerable()
            .AsParallel()
            .AsyncSelect(async x => {
                await Task.Delay(x);
                return x;
            })
            .ToListAsync();

        var time = watch.ElapsedMilliseconds;

        Assert.InRange(time, 280, 320);
        Assert.Equal([300, 200, 100], items);
    }
    
    [Fact]
    public async Task TestOrdered2() {
        var watch = new Stopwatch();
        watch.Start();
        
        var items = await new[] { 300, 200, 100 }
            .AsAsyncEnumerable()
            .AsParallel()
            .AsyncSelect(async x => {
                Thread.Sleep(x);
                return x;
            })
            .ToListAsync();

        var time = watch.ElapsedMilliseconds;

        Assert.InRange(time, 280, 320);
        Assert.Equal([300, 200, 100], items);
    }
    
    [Fact]
    public async Task TestUnordered1() {
        var watch = new Stopwatch();
        watch.Start();
        
        var items = await new[] { 300, 200, 100 }
            .AsAsyncEnumerable()
            .AsParallel(false)
            .AsyncSelect(async x => {
                await Task.Delay(x);
                return x;
            })
            .ToListAsync();

        var time = watch.ElapsedMilliseconds;

        Assert.InRange(time, 280, 320);
        Assert.Equal([100, 200, 300], items);
    }
    
    [Fact]
    public async Task TestUnordered2() {
        var watch = new Stopwatch();
        watch.Start();
        
        var items = await new[] { 300, 200, 100 }
            .AsAsyncEnumerable()
            .AsParallel(false)
            .AsyncSelect(async x => {
                Thread.Sleep(x);
                return x;
            })
            .ToListAsync();

        var time = watch.ElapsedMilliseconds;

        Assert.InRange(time, 280, 320);
        Assert.Equal([100, 200, 300], items);
    }
}