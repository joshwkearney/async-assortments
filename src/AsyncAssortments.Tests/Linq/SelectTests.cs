using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class SelectTests {
    [Fact]
    public void TestNullInputs() {
        var seq = new TestEnumerable<int>([1, 2, 3]);
        
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().Select(x => x + 1));
        Assert.Throws<ArgumentNullException>(() => seq.Select<int, int>(null!));
    }
    
    [Fact]
    public async Task TestExceptions() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Select<int, int>(x => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await seq.ToListAsync());
    }
    
    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Select(x => x / 2 - 1).ToArray();
        
        var test = await new TestEnumerable<int>(list)
            .Select(x => x / 2 - 1)
            .ToListAsync();
        
        Assert.Equal(expected, test);
    }
}