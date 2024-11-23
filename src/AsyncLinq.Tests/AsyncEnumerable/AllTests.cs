using AsyncCollections.Linq;

namespace AsyncCollections.Linq.Tests;

public class AllTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        var seq = new TestEnumerable<int>([1, 2, 3]);

        Assert.Throws<ArgumentNullException>(() => nullSeq.AllAsync(x => x > 2));
        Assert.Throws<ArgumentNullException>(() => seq.AnyAsync(null));
    }

    [Fact]
    public async Task TestExceptions() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Select<int, int>((x) => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await seq.AllAsync(x => x > 3));
    }
    
    [Fact]
    public async Task TestSequence() {
        var seq = new TestEnumerable<int>([1, 2, 3, 4]);
        
        Assert.True(await seq.AllAsync(x => x > 0));
        Assert.True(await seq.AllAsync(x => x < 5));
        Assert.False(await seq.AllAsync(x => x == 1));
    }
    
    [Fact]
    public async Task TestEmptySequence() {
        Assert.True(await AsyncEnumerable.Empty<int>().AllAsync(x => x >= 10));
    }
    
    [Fact]
    public async Task TestCancellation() {
        var seq = new TestEnumerable<int>([1, 2, 3]);

        await Assert.ThrowsAsync<TaskCanceledException>(async () => {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();

            var task = seq.AnyAsync(x => x >= 2, tokenSource.Token);
            await task;
        });
    }
}