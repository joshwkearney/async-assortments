using AsyncAssortments.Linq;

namespace AsyncAssortments.Linq.Tests;

public class ConcatTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        var seq = new TestEnumerable<int>([1, 2, 3]);

        Assert.Throws<ArgumentNullException>(() => nullSeq.Concat([]));
        Assert.Throws<ArgumentNullException>(() => nullSeq.Concat(new TestEnumerable<int>([])));

        Assert.Throws<ArgumentNullException>(() => seq.Concat(null as IAsyncEnumerable<int>));
        Assert.Throws<ArgumentNullException>(() => seq.Concat(null as IEnumerable<int>));
    }

    [Fact]
    public async Task TestRandomSequence1() {
        var list1 = TestHelper.CreateRandomList(20);
        var list2 = TestHelper.CreateRandomList(20);

        var expected = list1.Concat(list2).ToArray();
        var test = await new TestEnumerable<int>(list1).Concat(new TestEnumerable<int>(list2)).ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence2() {
        var list1 = TestHelper.CreateRandomList(20);
        var list2 = TestHelper.CreateRandomList(20);

        var expected = list1.Concat(list2).ToArray();
        var test = await new TestEnumerable<int>(list1).Concat(list2).ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence3() {
        var list1 = TestHelper.CreateRandomList(20);
        var list2 = TestHelper.CreateRandomList(20);

        var expected = list1.Concat(list2).ToArray();

        var test = await new TestEnumerable<int>(list1)
            .AsConcurrent(true)
            .Concat(new TestEnumerable<int>(list2))
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence4() {
        var list1 = TestHelper.CreateRandomList(20);
        var list2 = TestHelper.CreateRandomList(20);

        var expected = list1.Concat(list2).ToArray();

        var test = await new TestEnumerable<int>(list1)
            .AsConcurrent(true)
            .Concat(list2)
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence5() {
        var list1 = TestHelper.CreateRandomList(20);
        var list2 = TestHelper.CreateRandomList(20);

        var expected = list1.Concat(list2).ToArray();

        var test = await new TestEnumerable<int>(list1)
            .AsParallel(true)
            .Concat(new TestEnumerable<int>(list2))
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestRandomSequence6() {
        var list1 = TestHelper.CreateRandomList(20);
        var list2 = TestHelper.CreateRandomList(20);

        var expected = list1.Concat(list2).ToArray();

        var test = await new TestEnumerable<int>(list1)
            .AsParallel(true)
            .Concat(list2)
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestEmptySequence() {
        var seq = new[] { 1, 2, 3 };

        var elements = await AsyncEnumerable.Empty<int>()
            .Concat(new TestEnumerable<int>(seq))
            .Concat(AsyncEnumerable.Empty<int>())
            .ToListAsync();

        Assert.Equal(seq, elements);
    }

    [Fact]
    public async Task TestInterleaved1() {
        var seq = await GetSeq1().AsConcurrent(false).Concat(GetSeq2()).ToListAsync();

        Assert.Equal([1, 2, 3, 4, 5, 6], seq);

        async IAsyncEnumerable<int> GetSeq1() {
            await Task.Delay(100);
            yield return 2;

            await Task.Delay(100);
            yield return 4;

            await Task.Delay(100);
            yield return 6;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            await Task.Delay(50);
            yield return 1;

            await Task.Delay(100);
            yield return 3;

            await Task.Delay(100);
            yield return 5;
        }
    }

    [Fact]
    public async Task TestInterleaved2() {
        var seq = await GetSeq1().AsParallel(false).Concat(GetSeq2()).ToListAsync();

        Assert.Equal([1, 2, 3, 4, 5, 6], seq);

        async IAsyncEnumerable<int> GetSeq1() {
            await Task.Delay(100);
            yield return 2;

            await Task.Delay(100);
            yield return 4;

            await Task.Delay(100);
            yield return 6;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            await Task.Delay(50);
            yield return 1;

            await Task.Delay(100);
            yield return 3;

            await Task.Delay(100);
            yield return 5;
        }
    }

    [Fact]
    public async Task TestExceptions1() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Concat(GetBad());

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions2() {
        var seq = GetBad().Concat(new TestEnumerable<int>([1, 2, 3]));

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions3() {
        var seq = new TestEnumerable<int>([1, 2, 3]).AsConcurrent(true).Concat(GetBad());

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions4() {
        var seq = GetBad().AsConcurrent(true).Concat(new TestEnumerable<int>([1, 2, 3]));

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions5() {
        var seq = new TestEnumerable<int>([1, 2, 3]).AsConcurrent(false).Concat(GetBad());

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions6() {
        var seq = GetBad().AsConcurrent(false).Concat(new TestEnumerable<int>([1, 2, 3]));

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions7() {
        var seq = new TestEnumerable<int>([1, 2, 3]).AsParallel(true).Concat(GetBad());

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions8() {
        var seq = GetBad().AsParallel(true).Concat(new TestEnumerable<int>([1, 2, 3]));

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions9() {
        var seq = new TestEnumerable<int>([1, 2, 3]).AsParallel(false).Concat(GetBad());

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions10() {
        var seq = GetBad().AsParallel(false).Concat(new TestEnumerable<int>([1, 2, 3]));

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions11() {
        var seq = GetSeq().AsConcurrent(true).Concat(GetSeq());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetSeq() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions12() {
        var seq = GetSeq().AsConcurrent(false).Concat(GetSeq());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetSeq() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions13() {
        var seq = GetSeq().AsParallel(true).Concat(GetSeq());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetSeq() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TestExceptions14() {
        var seq = GetSeq().AsParallel(false).Concat(GetSeq());

        await Assert.ThrowsAsync<AggregateException>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetSeq() {
            throw new TestException();
            yield break;
        }
    }

    [Fact]
    public async Task TimeOrdered1() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await GetSeq1()
                .Concat(GetSeq2())
                .Concat(GetSeq3())
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([100, 200, 300], items);

        async IAsyncEnumerable<int> GetSeq1() {
            await Task.Delay(100);
            yield return 100;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            await Task.Delay(200);
            yield return 200;
        }

        async IAsyncEnumerable<int> GetSeq3() {
            await Task.Delay(300);
            yield return 300;
        }
    }

    [Fact]
    public async Task TimeOrdered2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await AsyncEnumerable.Empty<int>()
                .AsConcurrent(true)
                .Concat(GetSeq1())
                .Concat(GetSeq2())
                .Concat(GetSeq3())
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);

        async IAsyncEnumerable<int> GetSeq1() {
            await Task.Delay(100);
            yield return 100;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            await Task.Delay(200);
            yield return 200;
        }

        async IAsyncEnumerable<int> GetSeq3() {
            await Task.Delay(300);
            yield return 300;
        }
    }

    [Fact]
    public async Task TimeOrdered3() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await AsyncEnumerable.Empty<int>()
                .AsConcurrent(true)
                .Concat(GetSeq1())
                .Concat(GetSeq2())
                .Concat(GetSeq3())
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([100, 200, 300], items);

        async IAsyncEnumerable<int> GetSeq1() {
            Thread.Sleep(100);
            yield return 100;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            Thread.Sleep(200);
            yield return 200;
        }

        async IAsyncEnumerable<int> GetSeq3() {
            Thread.Sleep(300);
            yield return 300;
        }
    }

    [Fact]
    public async Task TimeOrdered4() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await AsyncEnumerable.Empty<int>()
                .AsParallel(true)
                .Concat(GetSeq1())
                .Concat(GetSeq2())
                .Concat(GetSeq3())
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);

        async IAsyncEnumerable<int> GetSeq1() {
            await Task.Delay(100);
            yield return 100;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            await Task.Delay(200);
            yield return 200;
        }

        async IAsyncEnumerable<int> GetSeq3() {
            await Task.Delay(300);
            yield return 300;
        }
    }

    [Fact]
    public async Task TimeOrdered5() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await AsyncEnumerable.Empty<int>()
                .AsParallel(true)
                .Concat(GetSeq1())
                .Concat(GetSeq2())
                .Concat(GetSeq3())
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);

        async IAsyncEnumerable<int> GetSeq1() {
            Thread.Sleep(100);
            yield return 100;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            Thread.Sleep(200);
            yield return 200;
        }

        async IAsyncEnumerable<int> GetSeq3() {
            Thread.Sleep(300);
            yield return 300;
        }
    }

    [Fact]
    public async Task TimeUnordered1() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await AsyncEnumerable.Empty<int>()
                .AsConcurrent(false)
                .Concat(GetSeq3())
                .Concat(GetSeq2())
                .Concat(GetSeq1())
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);

        async IAsyncEnumerable<int> GetSeq1() {
            await Task.Delay(100);
            yield return 100;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            await Task.Delay(200);
            yield return 200;
        }

        async IAsyncEnumerable<int> GetSeq3() {
            await Task.Delay(300);
            yield return 300;
        }
    }

    [Fact]
    public async Task TimeUnordered2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await AsyncEnumerable.Empty<int>()
                .AsConcurrent(false)
                .Concat(GetSeq3())
                .Concat(GetSeq2())
                .Concat(GetSeq1())
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 200, 100], items);

        async IAsyncEnumerable<int> GetSeq1() {
            Thread.Sleep(100);
            yield return 100;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            Thread.Sleep(200);
            yield return 200;
        }

        async IAsyncEnumerable<int> GetSeq3() {
            Thread.Sleep(300);
            yield return 300;
        }
    }

    [Fact]
    public async Task TimeUnordered3() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await AsyncEnumerable.Empty<int>()
                .AsParallel(false)
                .Concat(GetSeq3())
                .Concat(GetSeq2())
                .Concat(GetSeq1())
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);

        async IAsyncEnumerable<int> GetSeq1() {
            await Task.Delay(100);
            yield return 100;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            await Task.Delay(200);
            yield return 200;
        }

        async IAsyncEnumerable<int> GetSeq3() {
            await Task.Delay(300);
            yield return 300;
        }
    }

    [Fact]
    public async Task TimeUnordered4() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await AsyncEnumerable.Empty<int>()
                .AsParallel(false)
                .Concat(GetSeq3())
                .Concat(GetSeq2())
                .Concat(GetSeq1())
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 200, 300], items);

        async IAsyncEnumerable<int> GetSeq1() {
            Thread.Sleep(100);
            yield return 100;
        }

        async IAsyncEnumerable<int> GetSeq2() {
            Thread.Sleep(200);
            yield return 200;
        }

        async IAsyncEnumerable<int> GetSeq3() {
            Thread.Sleep(300);
            yield return 300;
        }
    }
}