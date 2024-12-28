using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class CountTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().CountAsync());
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