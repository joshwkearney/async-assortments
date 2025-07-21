
using System.Collections.Concurrent;
using AsyncAssortments;
using AsyncAssortments.Linq;
namespace AsyncLinq.Tests.Linq;

public class AsyncPrependTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsyncPrepend(() => ValueTask.FromResult(45)));
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsyncPrepend(c => ValueTask.FromResult(45)));
    }
    
    [Fact]
    public async Task TestOrdered() {
        var expected = new[] { 99, 1, 2, 3, 4 };

        var seq1 = new[] { 1, 2, 3, 4 }.ToAsyncEnumerable().AsyncPrepend(CreateItem1);
        var seq2 = new[] { 1, 2, 3, 4 }.ToAsyncEnumerable().AsyncPrepend(CreateItem2);
        var seq3 = new TestEnumerable<int>([1, 2, 3, 4]).AsyncPrepend(CreateItem1);
        var seq4 = new TestEnumerable<int>([1, 2, 3, 4]).AsyncPrepend(CreateItem2);
        var seq5 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent().AsyncPrepend(CreateItem1);
        var seq6 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent().AsyncPrepend(CreateItem2);
        var seq7 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel().AsyncPrepend(CreateItem1);
        var seq8 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel().AsyncPrepend(CreateItem2);
        
        var elements1 = await seq1.ToListAsync();
        var elements2 = await seq2.ToListAsync();
        var elements3 = await seq3.ToListAsync();
        var elements4 = await seq4.ToListAsync();
        var elements5 = await seq5.ToListAsync();
        var elements6 = await seq6.ToListAsync();
        var elements7 = await seq7.ToListAsync();
        var elements8 = await seq8.ToListAsync();

        Assert.Equal(expected, elements1);
        Assert.Equal(expected, elements2);
        Assert.Equal(expected, elements3);
        Assert.Equal(expected, elements4);
        Assert.Equal(expected, elements5);
        Assert.Equal(expected, elements6);
        Assert.Equal(expected, elements7);
        Assert.Equal(expected, elements8);

        async ValueTask<int> CreateItem1() {
            await Task.Delay(100);
            return 99;
        }
        
        async ValueTask<int> CreateItem2(CancellationToken token) {
            await Task.Delay(100, token);
            return 99;
        }
    }
    
    [Fact]
    public async Task TestUnordered() {
        var expected = new[] { 99, 1, 2, 3, 4 };

        var seq1 = new[] { 1, 2, 3, 4 }.ToAsyncEnumerable().AsyncPrepend(CreateItem1);
        var seq2 = new[] { 1, 2, 3, 4 }.ToAsyncEnumerable().AsyncPrepend(CreateItem2);
        var seq3 = new TestEnumerable<int>([1, 2, 3, 4]).AsyncPrepend(CreateItem1);
        var seq4 = new TestEnumerable<int>([1, 2, 3, 4]).AsyncPrepend(CreateItem2);
        var seq5 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent(false).AsyncPrepend(CreateItem1);
        var seq6 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent(false).AsyncPrepend(CreateItem2);
        var seq7 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel(false).AsyncPrepend(CreateItem1);
        var seq8 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel(false).AsyncPrepend(CreateItem2);
        
        var elements1 = await seq1.ToListAsync();
        var elements2 = await seq2.ToListAsync();
        var elements3 = await seq3.ToListAsync();
        var elements4 = await seq4.ToListAsync();
        var elements5 = await seq5.ToListAsync();
        var elements6 = await seq6.ToListAsync();
        var elements7 = await seq7.ToListAsync();
        var elements8 = await seq8.ToListAsync();

        Assert.Equal(expected, elements1);
        Assert.Equal(expected, elements2);
        Assert.Equal(expected, elements3);
        Assert.Equal(expected, elements4);
        Assert.Equal(expected, elements5);
        Assert.Equal(expected, elements6);
        Assert.Equal(expected, elements7);
        Assert.Equal(expected, elements8);

        async ValueTask<int> CreateItem1() {
            await Task.Delay(100);
            return 99;
        }
        
        async ValueTask<int> CreateItem2(CancellationToken token) {
            await Task.Delay(100, token);
            return 99;
        }
    }
    
    [Fact]
    public async Task TestEmptySequence() {
        var elements = await AsyncEnumerable.Empty<int>().AsyncPrepend(CreateItem).ToListAsync();
        
        Assert.Equal([99], elements);
        
        async ValueTask<int> CreateItem() {
            await Task.Delay(10);
            return 99;
        }
    }
    
    [Fact]
    public async Task TestExceptions1() {
        await Assert.ThrowsAsync<TestException>(async () => {
            await AsyncEnumerable.Empty<int>()
                .AsyncPrepend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .ToListAsync();
        });
    }
    
    [Fact]
    public async Task TestExceptions2() {
        await Assert.ThrowsAsync<AggregateException>(async () => {
            await AsyncEnumerable.Empty<int>()
                .AsConcurrent(false)
                .AsyncPrepend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .AsyncPrepend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .ToListAsync();
        });
    }
    
    [Fact]
    public async Task TestExceptions3() {
        await Assert.ThrowsAsync<AggregateException>(async () => {
            await AsyncEnumerable.Empty<int>()
                .AsConcurrent(true)
                .AsyncPrepend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .AsyncPrepend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .ToListAsync();
        });
    }
    
    [Fact]
    public async Task TestExceptions4() {
        await Assert.ThrowsAsync<AggregateException>(async () => {
            await AsyncEnumerable.Empty<int>()
                .AsParallel(true)
                .AsyncPrepend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .AsyncPrepend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .ToListAsync();
        });
    }
    
    [Fact]
    public async Task TestExceptions5() {
        await Assert.ThrowsAsync<AggregateException>(async () => {
            await AsyncEnumerable.Empty<int>()
                .AsParallel(false)
                .AsyncPrepend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .AsyncPrepend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .ToListAsync();
        });
    }
    
    [Fact]
    public async Task TestCancellation() {
        var seq = AsyncEnumerable.Empty<int>()
            .AsyncPrepend(async token => {
                await Task.Delay(1000, token);
                return 99;
            });

        await Assert.ThrowsAsync<TaskCanceledException>(async () => {
            var cancellation = new CancellationTokenSource();
            var task = seq.ToListAsync(cancellation.Token);

            await cancellation.CancelAsync();
            await task;
        });
    }
    
    [Fact]
    public async Task TestConcurrent1() {
        var (time, items) = await TestHelper.TimeAsync(async () => { 
            return await AsyncEnumerable.Empty<int>()
                .AsConcurrent(true)
                .AsyncPrepend(async () => { await Task.Delay(100); return 100; })
                .AsyncPrepend(async () => { await Task.Delay(200); return 200; })
                .AsyncPrepend(async () => { await Task.Delay(300); return 300; })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TestConcurrent2() {
        var (time, items) = await TestHelper.TimeAsync(async () => { 
            return await AsyncEnumerable.Empty<int>()
                .AsConcurrent(false)
                .AsyncPrepend(async () => { await Task.Delay(100); return 100; })
                .AsyncPrepend(async () => { await Task.Delay(200); return 200; })
                .AsyncPrepend(async () => { await Task.Delay(300); return 300; })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }
    
    [Fact]
    public async Task TestConcurrent3() {
        var (time, items) = await TestHelper.TimeAsync(async () => { 
            return await AsyncEnumerable.Empty<int>()
                .AsConcurrent(true)
                .AsyncPrepend(() => { Thread.Sleep(100); return ValueTask.FromResult(100); })
                .AsyncPrepend(() => { Thread.Sleep(200); return ValueTask.FromResult(200); })
                .AsyncPrepend(() => { Thread.Sleep(300); return ValueTask.FromResult(300); })
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 200, 100], items);
    }
    
    [Fact]
    public async Task TestConcurrent4() {
        var (time, items) = await TestHelper.TimeAsync(async () => { 
            return await AsyncEnumerable.Empty<int>()
                .AsConcurrent(false)
                .AsyncPrepend(() => { Thread.Sleep(100); return ValueTask.FromResult(100); })
                .AsyncPrepend(() => { Thread.Sleep(200); return ValueTask.FromResult(200); })
                .AsyncPrepend(() => { Thread.Sleep(300); return ValueTask.FromResult(300); })
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 200, 100], items);
    }
    
    [Fact]
    public async Task TestParallel1() {
        var (time, items) = await TestHelper.TimeAsync(async () => { 
            return await AsyncEnumerable.Empty<int>()
                .AsParallel(true)
                .AsyncPrepend(async () => { await Task.Delay(100); return 100; })
                .AsyncPrepend(async () => { await Task.Delay(200); return 200; })
                .AsyncPrepend(async () => { await Task.Delay(300); return 300; })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }
    
    [Fact]
    public async Task TestParallel2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await AsyncEnumerable.Empty<int>()
                .AsParallel(false)
                .AsyncPrepend(async () => { await Task.Delay(100); return 100; })
                .AsyncPrepend(async () => { await Task.Delay(200); return 200; })
                .AsyncPrepend(async () => { await Task.Delay(300); return 300; })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }
    
    [Fact]
    public async Task TestParallel3() {
        var (time, items) = await TestHelper.TimeAsync(async () => { 
            return await AsyncEnumerable.Empty<int>()
                .AsParallel(true)
                .AsyncPrepend(() => { Thread.Sleep(100); return ValueTask.FromResult(100); })
                .AsyncPrepend(() => { Thread.Sleep(200); return ValueTask.FromResult(200); })
                .AsyncPrepend(() => { Thread.Sleep(300); return ValueTask.FromResult(300); })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }
    
    [Fact]
    public async Task TestParallel4() {
        var (time, items) = await TestHelper.TimeAsync(async () => { 
            return await AsyncEnumerable.Empty<int>()
                .AsParallel(false)
                .AsyncPrepend(() => { Thread.Sleep(100); return ValueTask.FromResult(100); })
                .AsyncPrepend(() => { Thread.Sleep(200); return ValueTask.FromResult(200); })
                .AsyncPrepend(() => { Thread.Sleep(300); return ValueTask.FromResult(300); })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }
}