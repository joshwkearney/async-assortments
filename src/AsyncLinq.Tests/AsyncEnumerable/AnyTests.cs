namespace AsyncLinq.Tests;

public class AnyTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;

        Assert.Throws<ArgumentNullException>(() => nullSeq.AnyAsync());
        Assert.Throws<ArgumentNullException>(() => nullSeq.AnyAsync());
    }

    [Fact]
    public async Task TestExceptions() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Select<int, int>((x) => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await seq.AnyAsync());
        await Assert.ThrowsAsync<TestException>(async () => await seq.AnyAsync(x => x > 10));
    }
    
    [Fact]
    public async Task TestSequence() {
        var seq = new TestEnumerable<int>([1, 2, 3, 4]);
        
        Assert.True(await seq.AnyAsync());
        Assert.True(await seq.AnyAsync(x => x >= 3));
        Assert.False(await seq.AnyAsync(x => x > 100));
    }
    
    [Fact]
    public async Task TestEmptySequence() {
        Assert.False(await AsyncEnumerable.Empty<int>().AnyAsync());
        Assert.False(await AsyncEnumerable.Empty<int>().AnyAsync(x => x >= 10));
    }
    
    [Fact]
    public async Task TestCancellation() {
        var seq = new TestEnumerable<int>([1, 2, 3]);

        await Assert.ThrowsAsync<TaskCanceledException>(async () => {
            var tokenSource = new CancellationTokenSource();

            await tokenSource.CancelAsync();
            await seq.AnyAsync(tokenSource.Token);
        });
        
        await Assert.ThrowsAsync<TaskCanceledException>(async () => {
            var tokenSource = new CancellationTokenSource();
            var task = seq.AnyAsync(x => x >= 2, tokenSource.Token);

            await tokenSource.CancelAsync();
            await task;
        });
    }
}