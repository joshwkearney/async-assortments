
using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class AsyncAppendTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsyncAppend(() => ValueTask.FromResult(45)));
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsyncAppend(c => ValueTask.FromResult(45)));
    }
    
    [Fact]
    public async Task TestOrdered() {
        var expected = new[] { 1, 2, 3, 4, 99 };

        var seq1 = new[] { 1, 2, 3, 4 }.ToAsyncEnumerable().AsyncAppend(CreateItem1);
        var seq2 = new[] { 1, 2, 3, 4 }.ToAsyncEnumerable().AsyncAppend(CreateItem2);
        var seq3 = new TestEnumerable<int>([1, 2, 3, 4]).AsyncAppend(CreateItem1);
        var seq4 = new TestEnumerable<int>([1, 2, 3, 4]).AsyncAppend(CreateItem2);
        var seq5 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent().AsyncAppend(CreateItem1);
        var seq6 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent().AsyncAppend(CreateItem2);
        var seq7 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel().AsyncAppend(CreateItem1);
        var seq8 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel().AsyncAppend(CreateItem2);
        
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
        // Here we're expecting the appended elements first because they can be yielded immediately, before
        // we're done waiting on the 1, 2, 3, 4
        var expected = new[] { -99, 1, 2, 3, 4, 99 };

        var seq1 = new TestEnumerable<int>([1, 2, 3, 4])
            .AsConcurrent(false)
            .AsyncAppend(CreateItemSlow1)
            .AsyncAppend(CreateItemFast1);
        
        var seq2 = new TestEnumerable<int>([1, 2, 3, 4])
            .AsConcurrent(false)
            .AsyncAppend(CreateItemSlow2)
            .AsyncAppend(CreateItemFast2);
        
        var seq3 = new TestEnumerable<int>([1, 2, 3, 4])
            .AsParallel(false)
            .AsyncAppend(CreateItemSlow1)
            .AsyncAppend(CreateItemFast1);
        
        var seq4 = new TestEnumerable<int>([1, 2, 3, 4])
            .AsParallel(false)
            .AsyncAppend(CreateItemSlow2)
            .AsyncAppend(CreateItemFast2);
        
        var elements1 = await seq1.ToListAsync();
        var elements2 = await seq2.ToListAsync();
        var elements3 = await seq3.ToListAsync();
        var elements4 = await seq4.ToListAsync();

        Assert.Equal(expected, elements1);
        Assert.Equal(expected, elements2);
        Assert.Equal(expected, elements3);
        Assert.Equal(expected, elements4);
        
        ValueTask<int> CreateItemFast1() => ValueTask.FromResult(-99);

        ValueTask<int> CreateItemFast2(CancellationToken token) => ValueTask.FromResult(-99);
        
        async ValueTask<int> CreateItemSlow1() {
            await Task.Delay(100);
            return 99;
        }
        
        async ValueTask<int> CreateItemSlow2(CancellationToken token) {
            await Task.Delay(100, token);
            return 99;
        }
    }
    
    [Fact]
    public async Task TestEmptySequence() {
        var elements = await AsyncEnumerable.Empty<int>().AsyncAppend(CreateItem).ToListAsync();
        
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
                .AsyncAppend(async token => {
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
                .AsyncAppend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .AsyncAppend(async token => {
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
                .AsyncAppend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .AsyncAppend(async token => {
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
                .AsyncAppend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .AsyncAppend(async token => {
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
                .AsyncAppend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .AsyncAppend(async token => {
                    await Task.Delay(100, token);
                    throw new TestException();
                })
                .ToListAsync();
        });
    }
    
    [Fact]
    public async Task TestCancellation() {
        var seq = AsyncEnumerable.Empty<int>()
            .AsyncAppend(async token => {
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
                .AsyncAppend(async () => { await Task.Delay(300); return 300; })
                .AsyncAppend(async () => { await Task.Delay(200); return 200; })
                .AsyncAppend(async () => { await Task.Delay(100); return 100; })
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
                .AsyncAppend(async () => { await Task.Delay(300); return 300; })
                .AsyncAppend(async () => { await Task.Delay(200); return 200; })
                .AsyncAppend(async () => { await Task.Delay(100); return 100; })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }
    
    [Fact]
    public async Task TestConcurrent3() {
        var (time, items) = await TestHelper.TimeAsync(async () => { 
            return await AsyncEnumerable.Empty<int>()
                .AsConcurrent(true)
                .AsyncAppend(() => { Thread.Sleep(300); return ValueTask.FromResult(300); })
                .AsyncAppend(() => { Thread.Sleep(200); return ValueTask.FromResult(200); })
                .AsyncAppend(() => { Thread.Sleep(100); return ValueTask.FromResult(100); })
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
                .AsyncAppend(() => { Thread.Sleep(300); return ValueTask.FromResult(300); })
                .AsyncAppend(() => { Thread.Sleep(200); return ValueTask.FromResult(200); })
                .AsyncAppend(() => { Thread.Sleep(100); return ValueTask.FromResult(100); })
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
                .AsyncAppend(async () => { await Task.Delay(300); return 300; })
                .AsyncAppend(async () => { await Task.Delay(200); return 200; })
                .AsyncAppend(async () => { await Task.Delay(100); return 100; })
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
                .AsyncAppend(async () => { await Task.Delay(300); return 300; })
                .AsyncAppend(async () => { await Task.Delay(200); return 200; })
                .AsyncAppend(async () => { await Task.Delay(100); return 100; })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }
    
    [Fact]
    public async Task TestParallel3() {
        var (time, items) = await TestHelper.TimeAsync(async () => { 
            return await AsyncEnumerable.Empty<int>()
                .AsParallel(true)
                .AsyncAppend(() => { Thread.Sleep(300); return ValueTask.FromResult(300); })
                .AsyncAppend(() => { Thread.Sleep(200); return ValueTask.FromResult(200); })
                .AsyncAppend(() => { Thread.Sleep(100); return ValueTask.FromResult(100); })
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
                .AsyncAppend(() => { Thread.Sleep(300); return ValueTask.FromResult(300); })
                .AsyncAppend(() => { Thread.Sleep(200); return ValueTask.FromResult(200); })
                .AsyncAppend(() => { Thread.Sleep(100); return ValueTask.FromResult(100); })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }
}