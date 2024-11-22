namespace AsyncLinq.Tests;

public class AppendTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;

        Assert.Throws<ArgumentNullException>(() => nullSeq.Append(10));
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
        // Here we're expecting the appended elements first because they can be yielded immediately, before
        // we're done waiting on the 1, 2, 3, 4
        var expected = new[] { 5, 6, 1, 2, 3, 4 };
        
        var seq1 = new TestEnumerable<int>([1, 2, 3, 4]).AsConcurrent(false).Append(5).Append(6);
        var seq2 = new TestEnumerable<int>([1, 2, 3, 4]).AsParallel(false).Append(5).Append(6);
        
        var elements1 = await seq1.ToListAsync();
        var elements2 = await seq2.ToListAsync();

        Assert.Equal(expected, elements1);
        Assert.Equal(expected, elements2);
    }
    
    [Fact]
    public async Task TestEmptySequence() {
        var elements = await AsyncEnumerable.Empty<int>().Append(99).ToListAsync();
        
        Assert.Equal([99], elements);
    }
}