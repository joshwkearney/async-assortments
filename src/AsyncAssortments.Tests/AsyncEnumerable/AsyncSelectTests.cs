
using AsyncAssortments.Linq;

namespace AsyncAssortments.Linq.Tests;

public class AsyncSelectTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        var seq = new TestEnumerable<int>([1, 2, 3]);

        Assert.Throws<ArgumentNullException>(() => nullSeq.AsyncSelect(async x => 45));
        Assert.Throws<ArgumentNullException>(() => nullSeq.AsyncSelect(async (c, x) => 45));

        Assert.Throws<ArgumentNullException>(() => seq.AsyncSelect(null as Func<int, CancellationToken, ValueTask<int>>));
        Assert.Throws<ArgumentNullException>(() => seq.AsyncSelect(null as Func<int, ValueTask<int>>));
    }

    [Fact]
    public async void TimeCancellation() {
        var seq = new TestEnumerable<int>([1, 2, 3])
            .AsyncSelect(async (x, c) => {
                await Task.Delay(1000, c);
                return x;
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
        var expected = list.Select(x => x / 2 - 1).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsyncSelect(async x => {
                await Task.Delay(5);
                return x / 2 - 1;
            })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence2() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Select(x => x / 2 - 1).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsConcurrent(true)
            .AsyncSelect(async x => {
                await Task.Delay(5);
                return x / 2 - 1;
            })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence3() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Select(x => x / 2 - 1).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsConcurrent(false)
            .AsyncSelect(async x => {
                await Task.Delay(5);
                return x / 2 - 1;
            })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence4() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Select(x => x / 2 - 1).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsParallel(true)
            .AsyncSelect(async x => {
                await Task.Delay(5);
                return x / 2 - 1;
            })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence5() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Select(x => x / 2 - 1).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsParallel(false)
            .AsyncSelect(async x => {
                await Task.Delay(5);
                return x / 2 - 1;
            })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestEmptySequence() {
        var elements = await AsyncEnumerable.Empty<int>().Select(x => x + 1).ToListAsync();

        Assert.Equal([], elements);
    }

    [Fact]
    public async Task TestExceptions1() {
        var seq = new TestEnumerable<int>([1, 2, 3]).AsyncSelect<int, int>(x => throw new TestException());

        await Assert.ThrowsAsync<TestException>(async () => await seq.ToListAsync());
    }

    [Fact]
    public async Task TestExceptions2() {
        var seq = new TestEnumerable<int>([1, 2, 3])
            .AsConcurrent(true)
            .AsyncSelect<int, int>(x => throw new TestException());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());
    }

    [Fact]
    public async Task TestExceptions3() {
        var seq = new TestEnumerable<int>([1, 2, 3])
            .AsParallel(true)
            .AsyncSelect<int, int>(x => throw new TestException());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());
    }

    [Fact]
    public async Task TestExceptions4() {
        var seq = new TestEnumerable<int>([1, 2, 3])
            .AsConcurrent(false)
            .AsyncSelect<int, int>(x => throw new TestException());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());
    }

    [Fact]
    public async Task TestExceptions5() {
        var seq = new TestEnumerable<int>([1, 2, 3])
            .AsParallel(false)
            .AsyncSelect<int, int>(x => throw new TestException());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());
    }

    [Fact]
    public async Task TimeOrdered1() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(true)
                .AsyncSelect(async x => {
                    await Task.Delay(x);
                    return x;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeOrdered2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
             .ToAsyncEnumerable()
             .AsConcurrent(true)
             .AsyncSelect(async x => {
                 Thread.Sleep(x);
                 return x;
             })
             .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeOrdered3() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(true)
                .AsyncSelect(async x => {
                    await Task.Delay(x);
                    return x;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeOrdered4() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
             .ToAsyncEnumerable()
             .AsParallel(true)
             .AsyncSelect(async x => {
                 Thread.Sleep(x);
                 return x;
             })
             .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeUnordered1() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(false)
                .AsyncSelect(async x => {
                    await Task.Delay(x);
                    return x;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }

    [Fact]
    public async Task TimeUnordered2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(false)
                .AsyncSelect(async x => {
                    Thread.Sleep(x);
                    return x;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 200, 100], items);
    }

    [Fact]
    public async Task TimeUnordered3() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(false)
                .AsyncSelect(async x => {
                    await Task.Delay(x);
                    return x;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }

    [Fact]
    public async Task TimeUnordered4() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(false)
                .AsyncSelect(async x => {
                    Thread.Sleep(x);
                    return x;
                })
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);
    }
}