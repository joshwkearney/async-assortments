namespace AsyncLinq.Tests;

public class SelectManyTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        var seq = new TestEnumerable<int>([1, 2, 3]);
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.SelectMany(x => new[] { x, x }));
        Assert.Throws<ArgumentNullException>(() => nullSeq.SelectMany(x => new TestEnumerable<int>([x, x])));

        Assert.Throws<ArgumentNullException>(() => seq.SelectMany(null as Func<int, IEnumerable<int>>));
        Assert.Throws<ArgumentNullException>(() => seq.SelectMany(null as Func<int, IAsyncEnumerable<int>>));
    }

    [Fact]
    public async Task TestExceptions1() {
        var seq = new TestEnumerable<int>([1, 2, 3]).SelectMany(GetBad);
        
        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad(int i) {
            yield return i;
            yield return i;
            throw new TestException();
        }
    }

    [Fact]
    public async Task TestExceptions2() {
        var seq = new TestEnumerable<int>([1, 2, 3]).AsConcurrent(true).SelectMany(GetBad);

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad(int i) {
            yield return i;
            yield return i;
            throw new TestException();
        }
    }

    [Fact]
    public async Task TestExceptions3() {
        var seq = new TestEnumerable<int>([1, 2, 3]).AsConcurrent(false).SelectMany(GetBad);

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad(int i) {
            yield return i;
            yield return i;
            throw new TestException();
        }
    }

    [Fact]
    public async Task TestExceptions4() {
        var seq = new TestEnumerable<int>([1, 2, 3]).AsParallel(true).SelectMany(GetBad);

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad(int i) {
            yield return i;
            yield return i;
            throw new TestException();
        }
    }

    [Fact]
    public async Task TestExceptions5() {
        var seq = new TestEnumerable<int>([1, 2, 3]).AsParallel(false).SelectMany(GetBad);

        await Assert.ThrowsAnyAsync<Exception>(async () => await seq.ToListAsync());

        async IAsyncEnumerable<int> GetBad(int i) {
            yield return i;
            yield return i;
            throw new TestException();
        }
    }


    [Fact]
    public async Task TestEnumerable1() {
        var list = TestHelper.CreateRandomList(20);
        var expected = list.SelectMany(x => new[] { x, x }).ToArray();
        
        var test = await new TestEnumerable<int>(list)
            .SelectMany(x => new[] { x, x })
            .ToListAsync();
        
        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestEnumerable2() {
        var list = TestHelper.CreateRandomList(20);
        var expected = list.SelectMany(x => new[] { x, x }).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsConcurrent(true)
            .SelectMany(x => new[] { x, x })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestEnumerable3() {
        var list = TestHelper.CreateRandomList(20);
        var expected = list.SelectMany(x => new[] { x, x }).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsConcurrent(false)
            .SelectMany(x => new[] { x, x })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestEnumerable4() {
        var list = TestHelper.CreateRandomList(20);
        var expected = list.SelectMany(x => new[] { x, x }).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsParallel(true)
            .SelectMany(x => new[] { x, x })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestEnumerable5() {
        var list = TestHelper.CreateRandomList(20);
        var expected = list.SelectMany(x => new[] { x, x }).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsParallel(false)
            .SelectMany(x => new[] { x, x })
            .ToListAsync();

        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestProjection1() {
        var list = TestHelper.CreateRandomList(20);
        var expected = list.SelectMany(x => new[] { x, x }).ToArray();

        var test = await new TestEnumerable<int>(list)
            .SelectMany(Project)
            .ToListAsync();

        Assert.Equal(expected, test);

        async IAsyncEnumerable<int> Project(int i) {
            await Task.Delay(50);
            yield return i;
            await Task.Delay(100);
            yield return i;
        }
    }

    [Fact]
    public async Task TestProjection2() {
        var list = TestHelper.CreateRandomList(20);
        var expected = list.SelectMany(x => new[] { x, x }).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsConcurrent(true)
            .SelectMany(Project)
            .ToListAsync();

        Assert.Equal(expected, test);

        async IAsyncEnumerable<int> Project(int i) {
            await Task.Delay(50);
            yield return i;
            await Task.Delay(100);
            yield return i;
        }
    }

    [Fact]
    public async Task TestProjection3() {
        var list = TestHelper.CreateRandomList(20);
        var expected = list.SelectMany(x => new[] { x, x }).ToArray();

        var test = await new TestEnumerable<int>(list)
            .AsParallel(true)
            .SelectMany(Project)
            .ToListAsync();

        Assert.Equal(expected, test);

        async IAsyncEnumerable<int> Project(int i) {
            await Task.Delay(50);
            yield return i;
            await Task.Delay(100);
            yield return i;
        }
    }

    [Fact]
    public async Task TimeProjection1() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .SelectMany(Project)
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 300, 200, 200, 100, 100], items);

        async IAsyncEnumerable<int> Project(int i) {
            yield return i;
            await Task.Delay(i);
            yield return i;
        }
    }

    [Fact]
    public async Task TimeProjection2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(true)
                .SelectMany(Project)
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 300, 200, 200, 100, 100], items);

        async IAsyncEnumerable<int> Project(int i) {
            yield return i;
            await Task.Delay(i);
            yield return i;
        }
    }

    [Fact]
    public async Task TimeProjection3() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(true)
                .SelectMany(Project)
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 300, 200, 200, 100, 100], items);

        async IAsyncEnumerable<int> Project(int i) {
            yield return i;
            await Task.Delay(i);
            yield return i;
        }
    }

    [Fact]
    public async Task TimeProjection4() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(false)
                .SelectMany(Project)
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 100, 200, 200, 300, 300], items);

        async IAsyncEnumerable<int> Project(int i) {
            await Task.Delay(i);
            yield return i;
            yield return i;
        }
    }

    [Fact]
    public async Task TimeProjection5() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(false)
                .SelectMany(Project)
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 100, 200, 200, 300, 300], items);

        async IAsyncEnumerable<int> Project(int i) {
            await Task.Delay(i);
            yield return i;
            yield return i;
        }
    }

    [Fact]
    public async Task TimeBlockingProjection1() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(true)
                .SelectMany(Project)
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 300, 200, 200, 100, 100], items);

        async IAsyncEnumerable<int> Project(int i) {
            Thread.Sleep(i);
            yield return i;
            yield return i;
        }
    }

    [Fact]
    public async Task TimeBlockingProjection2() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(true)
                .SelectMany(Project)
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([300, 300, 200, 200, 100, 100], items);

        async IAsyncEnumerable<int> Project(int i) {
            Thread.Sleep(i);
            yield return i;
            yield return i;
        }
    }

    [Fact]
    public async Task TimeBlockingProjection3() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsConcurrent(false)
                .SelectMany(Project)
                .ToListAsync();
        });

        Assert.InRange(time, 550, 650);
        Assert.Equal([300, 300, 200, 200, 100, 100], items);

        async IAsyncEnumerable<int> Project(int i) {
            Thread.Sleep(i);
            yield return i;
            yield return i;
        }
    }

    [Fact]
    public async Task TimeBlockingProjection4() {
        var (time, items) = await TestHelper.TimeAsync(async () => {
            return await new[] { 300, 200, 100 }
                .ToAsyncEnumerable()
                .AsParallel(false)
                .SelectMany(Project)
                .ToListAsync();
        });

        Assert.InRange(time, 250, 350);
        Assert.Equal([100, 100, 200, 200, 300, 300], items);

        async IAsyncEnumerable<int> Project(int i) {
            Thread.Sleep(i);
            yield return i;
            yield return i;
        }
    }
}