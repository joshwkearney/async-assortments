namespace AsyncLinq.Tests;

public class ToHashSetTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.ToHashSetAsync());
    }
    
    [Fact]
    public async Task TestExceptions() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Select<int, int>(x => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await seq.ToHashSetAsync());
    }
    
    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(20);
        var expected = list.ToHashSet().OrderBy(x => x);
        var test = await new TestEnumerable<int>(list).ToHashSetAsync();
        var testList = test.OrderBy(x => x);
        
        Assert.Equal(expected, testList);
    }
}