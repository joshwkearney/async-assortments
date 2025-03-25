
using System.Collections.Concurrent;
using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class ForEachAsyncTests {
    [Fact]
    public void TestNullInputs() {
        var seq = new TestEnumerable<int>([1, 2, 3]);

        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().ForEachAsync(x => ValueTask.CompletedTask));
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().ForEachAsync((x, c) => ValueTask.CompletedTask));
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().ForEachAsync(x => { }));

        Assert.Throws<ArgumentNullException>(() => seq.ForEachAsync(GetNullSelector1()));
        Assert.Throws<ArgumentNullException>(() => seq.ForEachAsync(GetNullSelector2()));
        Assert.Throws<ArgumentNullException>(() => seq.ForEachAsync(GetNullSelector3()));

        static Func<int, ValueTask> GetNullSelector1() => null!;
        
        static Func<int, CancellationToken, ValueTask> GetNullSelector2() => null!;
        
        static Action<int> GetNullSelector3() => null!;
    }

    [Fact]
    public async void TimeCancellation() {
        var cancellation = new CancellationTokenSource();

        var (time, _) = await TestHelper.TimeAsync(async () => {
            try {
                var task = new TestEnumerable<int>([1, 2, 3]).ForEachAsync(
                    async (x, c) => {
                        await Task.Delay(1000, c);
                    }, 
                    cancellation.Token);
                
                await cancellation.CancelAsync();
                await task;
                
                return true;
            }
            catch (TaskCanceledException) {
                return false;
            }
        });

        Assert.InRange(time, 0, 50);
    }

    [Fact]
    public async Task TestRandomSequence1() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Order().ToList();
        var result = new ConcurrentBag<int>();

        await new TestEnumerable<int>(list).ForEachAsync(async x => {
            await Task.Delay(10);
            result.Add(x);
        });

        Assert.Equal(expected, result.Order());
    }

    [Fact]
    public async Task TestRandomSequence2() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Order().ToList();
        var result = new ConcurrentBag<int>();

        await new TestEnumerable<int>(list).ForEachAsync(x => {
            result.Add(x);
        });

        Assert.Equal(expected, result.Order());
    }

    [Fact]
    public async Task TestEmptySequence() {
        var expected = Enumerable.Empty<int>();
        var result = new ConcurrentBag<int>();

        await AsyncEnumerable.Empty<int>().ForEachAsync(x => result.Add(x));

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task TestExceptions1() {
        await Assert.ThrowsAsync<TestException>(async () => {
            await new TestEnumerable<int>([1, 2, 3]).ForEachAsync(x => throw new TestException());
        });
    }

    [Fact]
    public async Task TestExceptions2() {
        await Assert.ThrowsAsync<AggregateException>(async () => {
            await new TestEnumerable<int>([1, 2, 3])
                .AsConcurrent(true)
                .ForEachAsync(x => throw new TestException());
        });
    }
    
    [Fact]
    public async Task TestExceptions3() {
        await Assert.ThrowsAsync<AggregateException>(async () => {
            await new TestEnumerable<int>([1, 2, 3])
                .AsConcurrent(false)
                .ForEachAsync(x => throw new TestException());
        });
    }
    
    [Fact]
    public async Task TestExceptions4() {
        await Assert.ThrowsAsync<AggregateException>(async () => {
            await new TestEnumerable<int>([1, 2, 3])
                .AsParallel(true)
                .ForEachAsync(x => throw new TestException());
        });
    }
    
    [Fact]
    public async Task TestExceptions5() {
        await Assert.ThrowsAsync<AggregateException>(async () => {
            await new TestEnumerable<int>([1, 2, 3])
                .AsParallel(false)
                .ForEachAsync(x => throw new TestException());
        });
    }

    [Fact]
    public async Task TimeOrdered1() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            var result = new ConcurrentQueue<int>();
            
            await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(true)
                .ForEachAsync(async x => {
                    await Task.Delay(x);
                    result.Enqueue(x);
                });

            return result.ToArray();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }
    
    [Fact]
    public async Task TimeOrdered2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            var result = new ConcurrentQueue<int>();
            
            await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(true)
                .ForEachAsync(x => {
                    Thread.Sleep(x);
                    result.Enqueue(x);
                });

            return result.ToArray();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeOrdered3() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            var result = new ConcurrentQueue<int>();
            
            await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(true)
                .ForEachAsync(async x => {
                    await Task.Delay(x);
                    result.Enqueue(x);
                });

            return result.ToArray();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }

    [Fact]
    public async Task TimeOrdered4() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            var result = new ConcurrentQueue<int>();
            
            await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(true)
                .ForEachAsync(x => {
                    Thread.Sleep(x);
                    result.Enqueue(x);
                });

            return result.ToArray();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }
    
    [Fact]
    public async Task TimeUnordered1() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            var result = new ConcurrentQueue<int>();
            
            await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(false)
                .ForEachAsync(async x => {
                    await Task.Delay(x);
                    result.Enqueue(x);
                });

            return result.ToArray();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }
    
    [Fact]
    public async Task TimeUnordered2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            var result = new ConcurrentQueue<int>();
            
            await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(false)
                .ForEachAsync(x => {
                    Thread.Sleep(x);
                    result.Enqueue(x);
                });

            return result.ToArray();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 200, 100], items);
    }
    
    [Fact]
    public async Task TimeUnordered3() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            var result = new ConcurrentQueue<int>();
            
            await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(false)
                .ForEachAsync(async x => {
                    await Task.Delay(x);
                    result.Enqueue(x);
                });

            return result.ToArray();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }
    
    [Fact]
    public async Task TimeUnordered4() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            var result = new ConcurrentQueue<int>();
            
            await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(false)
                .ForEachAsync(x => {
                    Thread.Sleep(x);
                    result.Enqueue(x);
                });

            return result.ToArray();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }
}