using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class ToListTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().ToListAsync());
    }
    
    [Fact]
    public async Task TestExceptions() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Select<int, int>(x => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await seq.ToListAsync());
    }
    
    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(100);
        var test = await new TestEnumerable<int>(list).ToListAsync();
        
        Assert.Equal(list, test);
    }
}