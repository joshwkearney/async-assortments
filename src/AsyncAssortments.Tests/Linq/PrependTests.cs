using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class PrependTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().Prepend(10));
    }
    
    [Fact]
    public async Task TestOrdered() {
        var expected = new[] { 6, 5, 1, 2, 3, 4 };
        
        var seq1 = new TestEnumerable<int>([1, 2, 3, 4]).Prepend(5).Prepend(6);
        var seq2 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent().Prepend(5).Prepend(6);
        var seq3 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel().Prepend(5).Prepend(6);
        var seq4 = new[] { 1, 2, 3, 4 }.ToAsyncEnumerable().Prepend(5).Prepend(6);
        
        var elements1 = await seq1.ToListAsync();
        var elements2 = await seq2.ToListAsync();
        var elements3 = await seq3.ToListAsync();
        var elements4 = await seq4.ToListAsync();

        Assert.Equal(expected, elements1);
        Assert.Equal(expected, elements2);
        Assert.Equal(expected, elements3);
        Assert.Equal(expected, elements4);
    }
    
    [Fact]
    public async Task TestUnordered() {
        // Here we're expecting the appended elements first because they can be yielded immediately, before
        // we're done waiting on the 1, 2, 3, 4
        var expected = new[] { 6, 5, 1, 2, 3, 4 };
        
        var seq1 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent(false).Prepend(5).Prepend(6);
        var seq2 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel(false).Prepend(5).Prepend(6);
        
        var elements1 = await seq1.ToListAsync();
        var elements2 = await seq2.ToListAsync();

        Assert.Equal(expected, elements1);
        Assert.Equal(expected, elements2);
    }
    
    [Fact]
    public async Task TestEmptySequence() {
        var elements = await AsyncEnumerable.Empty<int>().Prepend(99).ToListAsync();
        
        Assert.Equal([99], elements);
    }
}