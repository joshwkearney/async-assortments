namespace AsyncLinq.Tests;

public class ToDictionaryTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.ToDictionaryAsync(x => x, x => x));
    }
    
    [Fact]
    public async Task TestExceptions() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Select<int, int>(x => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await seq.ToDictionaryAsync(x => x, x => x));
    }
    
    [Fact]
    public async Task TestRandomSequence() {
        var list = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

        var expected = list
            .ToDictionary(x => x * 2, x => x | 1)
            .OrderBy(x => x.Key)
            .ToArray();

        var test = await new TestEnumerable<int>(list).ToDictionaryAsync(x => x * 2, x => x | 1);
        var testList = test.OrderBy(x => x.Key).ToArray();
        
        Assert.Equal(expected, testList);
    }
}