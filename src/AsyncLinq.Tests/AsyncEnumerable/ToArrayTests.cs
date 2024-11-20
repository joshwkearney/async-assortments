namespace AsyncLinq.Tests;

public class ToArrayTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.ToArrayAsync());
    }
    
    [Fact]
    public async Task TestExceptions() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Select<int, int>(x => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await seq.ToArrayAsync());
    }
    
    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(100);
        var test = await new TestEnumerable<int>(list).ToArrayAsync();
        
        Assert.Equal(list, test);
    }
}