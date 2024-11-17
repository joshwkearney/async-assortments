namespace AsyncLinq.Tests;

public class AggregateTests {
    [Fact]
    public async Task TestSeedNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        var seq = new TestEnumerable<int>([1, 2, 3]);
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.AggregateAsync(0, (x, y) => x + y));
        Assert.Throws<ArgumentNullException>(() => seq.AggregateAsync(0, null));
        
        // This should not throw
        await seq.Select(x => (int?)x).AggregateAsync(null as int?, (x, y) => x + y);
    }
    
    [Fact]
    public async Task TestNoSeedNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        var seq = new TestEnumerable<int>([1, 2, 3]);
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.AggregateAsync((x, y) => x + y));
        Assert.Throws<ArgumentNullException>(() => seq.AggregateAsync(null));
    }
    
    [Fact]
    public async Task TestSeedExceptions() {
        var result1 = new TestEnumerable<int>([1, 2, 3]).AggregateAsync(0, (x, y) => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await result1);
    }
    
    [Fact]
    public async Task TestNoSeedExceptions() {
        var result2 = new TestEnumerable<int>([1, 2, 3]).AggregateAsync((x, y) => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await result2);
    }
    
    [Fact]
    public async Task TestSeedRandomSequence() {
        var list = TestHelper.CreateRandomList(100);

        var expected = list.Aggregate(37, (x, y) => 2 * x - y);
        var actual = await new TestEnumerable<int>(list).AggregateAsync(37, (x, y) => 2 * x - y);
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task TestNoSeedRandomSequence() {
        var list = TestHelper.CreateRandomList(100);

        var expected = list.Aggregate((x, y) => 2 * x - y);
        var actual = await new TestEnumerable<int>(list).AggregateAsync((x, y) => 2 * x - y);
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task TestNoSeedEmptySequences() {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.Empty<int>().AggregateAsync((x, y) => x + y));
    }
    
    [Fact]
    public async Task TestSeedEmptySequences() {
        var result = await AsyncEnumerable.Empty<int>().AggregateAsync(10, (x, y) => x + y);
        
        Assert.Equal(10, result);
    }
    
    [Fact]
    public async Task TestNoSeedCancellation() {
        var seq = new TestEnumerable<int>([1, 2, 3]);

        await Assert.ThrowsAsync<TaskCanceledException>(async () => {
            var tokenSource = new CancellationTokenSource();
            var task = seq.AggregateAsync((x, y) => x + y, tokenSource.Token);

            await tokenSource.CancelAsync();
            await task;
        });
    }
    
    [Fact]
    public async Task TestSeedCancellation() {
        var seq = new TestEnumerable<int>([1, 2, 3]);

        await Assert.ThrowsAsync<TaskCanceledException>(async () => {
            var tokenSource = new CancellationTokenSource();
            var task = seq.AggregateAsync(10, (x, y) => x + y, tokenSource.Token);

            await tokenSource.CancelAsync();
            await task;
        });
    }
}