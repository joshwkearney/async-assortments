
using System.Diagnostics;

namespace AsyncLinq.Tests;

public class AsyncWhereTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;

        Assert.Throws<ArgumentNullException>(() => nullSeq.AsyncWhere(async x => true));
        Assert.Throws<ArgumentNullException>(() => nullSeq.AsyncWhere(async (c, x) => true));
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
                .AsAsyncEnumerable()
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
                .AsAsyncEnumerable()
                .AsConcurrent(true)
                .AsyncWhere(async x => {
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
                .AsAsyncEnumerable()
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
                .AsAsyncEnumerable()
                .AsParallel(true)
                .AsyncWhere(async x => {
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
                .AsAsyncEnumerable()
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
                .AsAsyncEnumerable()
                .AsConcurrent(false)
                .AsyncWhere(async x => {
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
                .AsAsyncEnumerable()
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
                .AsAsyncEnumerable()
                .AsParallel(false)
                .AsyncWhere(async x => {
                    Thread.Sleep(x);
                    return x > 50;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }
}