using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class WhereTests {
    [Fact]
    public void TestNullInputs() {
        var seq = new TestEnumerable<int>([1, 2, 3]);
        
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().Where(x => x > 10));
        Assert.Throws<ArgumentNullException>(() => seq.Where(null!));
    }
    
    [Fact]
    public async Task TestExceptions() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Where(x => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await seq.ToListAsync());
    }
    
    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Select(x => x > 45).ToArray();
        
        var test = await new TestEnumerable<int>(list)
            .Select(x => x > 45)
            .ToListAsync();
        
        Assert.Equal(expected, test);
    }
}