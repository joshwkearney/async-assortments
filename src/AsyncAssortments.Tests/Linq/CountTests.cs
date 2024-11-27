using AsyncAssortments.Linq;

namespace AsyncAssortments.Linq.Tests;

public class CountTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.CountAsync());
    }
    
    [Fact]
    public async Task TestCancellation() {
        var seq = new TestEnumerable<int>([1, 2, 3]);
        var cancellation = new CancellationTokenSource();

        cancellation.Cancel();
        
        await Assert.ThrowsAsync<TaskCanceledException>(async () => await seq.CountAsync(cancellation.Token));
    }
    
    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(100);
        var test = new TestEnumerable<int>(list);
        
        Assert.Equal(list.Count, await test.CountAsync());
    }

    [Fact]
    public async Task TestPredicate() {
        var list = TestHelper.CreateRandomList(100);
        var test = new TestEnumerable<int>(list);
        var expected = list.Count(x => x > 35);

        Assert.Equal(expected, await test.CountAsync(x => x > 35));
    }
}