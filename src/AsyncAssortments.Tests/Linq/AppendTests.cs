using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class AppendTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().Append(10));
    }
    
    [Fact]
    public async Task TestOrdered() {
        var expected = new[] { 1, 2, 3, 4, 5, 6 };
        
        var seq1 = new TestEnumerable<int>([1, 2, 3, 4]).Append(5).Append(6);
        var seq2 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent().Append(5).Append(6);
        var seq3 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel().Append(5).Append(6);
        var seq4 = new[] { 1, 2, 3, 4 }.ToAsyncEnumerable().Append(5).Append(6);
        
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
        var expected = new[] { 1, 2, 3, 4, 5, 6 };
        
        var seq1 = new TestEnumerable<int>([1, 2, 3, 4]).Append(5).Append(6);
        var seq2 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent(false).Append(5).Append(6);
        var seq3 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel(false).Append(5).Append(6);
        var seq4 = new[] { 1, 2, 3, 4 }.ToAsyncEnumerable().Append(5).Append(6);
        
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
    public async Task TestEmptySequence() {
        var elements = await AsyncEnumerable.Empty<int>().Append(99).ToListAsync();
        
        Assert.Equal([99], elements);
    }
}