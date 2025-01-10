using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class ZipTests {
    [Fact]
    public void TestNullInputs() {
        var seq = new TestEnumerable<int>([1, 2, 3]);
        
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().Zip(seq));
        Assert.Throws<ArgumentNullException>(() => seq.Zip(null! as IAsyncEnumerable<int>));
        
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().Zip(seq, (x, y) => x + y));
        Assert.Throws<ArgumentNullException>(() => seq.Zip(null! as IAsyncEnumerable<int>, (x, y) => x + y));
        Assert.Throws<ArgumentNullException>(() => seq.Zip<int, int, int>(seq, null!));
    }
    
    [Fact]
    public async Task TestExceptions() {
        var seq = new TestEnumerable<int>([1, 2, 3]);
        
        await Assert.ThrowsAsync<TestException>(async () => await seq.Zip(TestHelper.GetFailingAsyncEnumerable()).ToListAsync());
        await Assert.ThrowsAsync<TestException>(async () => await TestHelper.GetFailingAsyncEnumerable().Zip(seq).ToListAsync());
    }
    
    [Fact]
    public async Task TestRandomSequence1() {
        var list1 = TestHelper.CreateRandomList(20);
        var list2 = TestHelper.CreateRandomList(20);
        var expected = list1.Zip(list2).ToArray();
        
        var test = await new TestEnumerable<int>(list1)
            .Zip(new TestEnumerable<int>(list2))
            .ToArrayAsync();
        
        Assert.Equal(expected, test);
    }
    
    [Fact]
    public async Task TestRandomSequence2() {
        var list1 = TestHelper.CreateRandomList(100);
        var list2 = TestHelper.CreateRandomList(100);
        var expected = list1.Zip(list2).ToArray();

        var test = await list1
            .ToAsyncEnumerable()
            .AsConcurrent()
            .Zip(list2)
            .ToArrayAsync();
        
        Assert.Equal(expected, test);
    }
}