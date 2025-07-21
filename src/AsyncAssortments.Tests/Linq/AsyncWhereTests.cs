
using System.Collections.Concurrent;
using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class AsyncWhereTests {
    [Fact]
    public void TestNullInputs() {
        var seq = new TestEnumerable<int>([1, 2, 3]);

        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsyncWhere(x => ValueTask.FromResult(true)));
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsyncWhere((x, c) => ValueTask.FromResult(true)));

        Assert.Throws<ArgumentNullException>(() => seq.AsyncWhere(GetAsyncPredicate1()));
        Assert.Throws<ArgumentNullException>(() => seq.AsyncWhere(GetAsyncPredicate2()));

        static Func<int, ValueTask<bool>> GetAsyncPredicate1() => null!;
        
        static Func<int, CancellationToken, ValueTask<bool>> GetAsyncPredicate2() => null!;
    }

    [Fact]
    public async void TimeCancellation() {
        var seq = new TestEnumerable<int>([1, 2, 3])
            .AsyncWhere(async (x, c) => {
                await Task.Delay(1000, c);
                return true;
            });

        var cancellation = new CancellationTokenSource();

        var (time, items) = await TestHelper.TimeAsync(async () => {
            var task = seq.ToListAsync(cancellation.Token);

            try {
                await cancellation.CancelAsync();
                return await task;
            }
            catch (TaskCanceledException) {
                return [];
            }
        });

        Assert.InRange(time, 0, 50);
    }

    [Fact]
    public async Task TestRandomSequence1() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Where(x => x >= 35).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsyncWhere(async x => {
                await Task.Delay(5);
                return x >= 35;
            })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence2() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Where(x => x >= 35).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsConcurrent(true)
            .AsyncWhere(async x => {
                await Task.Delay(5);
                return x >= 35;
            })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence3() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Where(x => x >= 35).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsConcurrent(false)
            .AsyncWhere(async x => {
                await Task.Delay(5);
                return x >= 35;
            })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence4() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Where(x => x >= 35).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsParallel(true)
            .AsyncWhere(async x => {
                await Task.Delay(5);
                return x >= 35;
            })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence5() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Where(x => x >= 35).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsParallel(false)
            .AsyncWhere(async x => {
                await Task.Delay(5);
                return x >= 35;
            })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestEmptySequence() {
        var elements = await AsyncEnumerable.Empty<int>().Where(x => x > 1).ToListAsync();

        Assert.Equal([], elements);
    }

    [Fact]
    public async Task TestExceptions1() {
        var seq = new TestEnumerable<int>([1, 2, 3]).AsyncWhere(x => throw new TestException());

        await Assert.ThrowsAsync<TestException>(async () => await seq.ToListAsync());
    }

    [Fact]
    public async Task TestExceptions2() {
        var seq = new TestEnumerable<int>([1, 2, 3])
            .AsConcurrent(true)
            .AsyncWhere(x => throw new TestException());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());
    }

    [Fact]
    public async Task TestExceptions3() {
        var seq = new TestEnumerable<int>([1, 2, 3])
            .AsParallel(true)
            .AsyncWhere(x => throw new TestException());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());
    }

    [Fact]
    public async Task TestExceptions4() {
        var seq = new TestEnumerable<int>([1, 2, 3])
            .AsConcurrent(false)
            .AsyncWhere(x => throw new TestException());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());
    }

    [Fact]
    public async Task TestExceptions5() {
        var seq = new TestEnumerable<int>([1, 2, 3])
            .AsParallel(false)
            .AsyncWhere(x => throw new TestException());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());
    }

    [Fact]
    public async Task TimeOrdered1() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100, 0 }
                .ToAsyncEnumerable()
                .AsConcurrent(true)
                .AsyncWhere(async x => {
                    await Task.Delay(x);
                    return x > 50;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeOrdered2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100, 0 }
                .ToAsyncEnumerable()
                .AsConcurrent(true)
                .AsyncWhere(async x => {
                    await Task.CompletedTask;
                    Thread.Sleep(x);
                    return x > 50;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeOrdered3() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100, 0 }
                .ToAsyncEnumerable()
                .AsParallel(true)
                .AsyncWhere(async x => {
                    await Task.Delay(x);
                    return x > 50;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeOrdered4() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100, 0 }
                .ToAsyncEnumerable()
                .AsParallel(true)
                .AsyncWhere(async x => {
                    await Task.CompletedTask;
                    Thread.Sleep(x);
                    return x > 50;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeUnordered1() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100, 0 }
                .ToAsyncEnumerable()
                .AsConcurrent(false)
                .AsyncWhere(async x => {
                    await Task.Delay(x);
                    return x > 50;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }

    [Fact]
    public async Task TimeUnordered2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100, 0 }
                .ToAsyncEnumerable()
                .AsConcurrent(false)
                .AsyncWhere(async x => {
                    await Task.CompletedTask;
                    Thread.Sleep(x);
                    return x > 50;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeUnordered3() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100, 0 }
                .ToAsyncEnumerable()
                .AsParallel(false)
                .AsyncWhere(async x => {
                    await Task.Delay(x);
                    return x > 50;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }

    [Fact]
    public async Task TimeUnordered4() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100, 0 }
                .ToAsyncEnumerable()
                .AsParallel(false)
                .AsyncWhere(async x => {
                    await Task.CompletedTask;
                    Thread.Sleep(x);
                    return x > 50;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }

    [Fact]
    public async Task TestMaxConcurrencyOrdered() {
        int concurrent = 0;
        var reads = new ConcurrentBag<int>();

        await TestHelper.CreateRandomList(20)
            .ToAsyncEnumerable()
            .AsConcurrent(preserveOrder: true, maxConcurrency: 10)
            .AsyncWhere(async x => {
                try {
                    reads.Add(Interlocked.Increment(ref concurrent));
                    await Task.Delay(10);

                    return true;
                }
                finally {
                    Interlocked.Decrement(ref concurrent);
                }
            })
            .ToListAsync();

        Assert.Equal(10, reads.Max());
    }

    [Fact]
    public async Task TestMaxConcurrencyUnordered() {
        int concurrent = 0;
        var reads = new ConcurrentBag<int>();

        await TestHelper.CreateRandomList(20)
            .ToAsyncEnumerable()
            .AsConcurrent(preserveOrder: false, maxConcurrency: 10)
            .AsyncWhere(async x => {
                try {
                    reads.Add(Interlocked.Increment(ref concurrent));
                    await Task.Delay(10);

                    return true;
                }
                finally {
                    Interlocked.Decrement(ref concurrent);
                }
            })
            .ToListAsync();

        Assert.Equal(10, reads.Max());
    }
}