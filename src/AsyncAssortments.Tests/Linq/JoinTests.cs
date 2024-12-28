using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class JoinTests {
    [Fact]
    public void TestNullInputs() {
        var seq1 = new TestEnumerable<int>([1, 2, 3]);
        var seq2 = new TestEnumerable<int>([1, 2, 3]);
        
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().Join(seq2, x => x, x => x));
        Assert.Throws<ArgumentNullException>(() => seq1.Join(TestHelper.GetNullAsyncEnumerable(), x => x, x => x, (x, y) => x + y, EqualityComparer<int>.Default));
        Assert.Throws<ArgumentNullException>(() => seq1.Join(seq2, null!, x => x, (x, y) => x + y, EqualityComparer<int>.Default));
        Assert.Throws<ArgumentNullException>(() => seq1.Join(seq2, x => x, null!, (x, y) => x + y, EqualityComparer<int>.Default));
        Assert.Throws<ArgumentNullException>(() => seq1.Join<int, int, int, int>(seq2, x => x, x => x, null!, EqualityComparer<int>.Default));
        Assert.Throws<ArgumentNullException>(() => seq1.Join(seq2, x => x, x => x, (x, y) => x + y, null!));
    }
    
    [Fact]
    public async Task TestExceptions1() {
        var seq1 = TestHelper.GetFailingAsyncEnumerable();
        var seq2 = new TestEnumerable<int>([1, 2, 3]);
        
        await Assert.ThrowsAsync<TestException>(async () => await seq1.Join(seq2, x => x, x => x).ToListAsync());
    }
    
    [Fact]
    public async Task TestExceptions2() {
        var seq1 = TestHelper.GetFailingAsyncEnumerable();
        var seq2 = new TestEnumerable<int>([1, 2, 3]);
        
        await Assert.ThrowsAsync<TestException>(async () => await seq2.Join(seq1, x => x, x => x).ToListAsync());
    }
    
    [Fact]
    public async Task TestRandomSequences() {
        var list1 = TestHelper.CreateRandomList(100);
        var list2 = TestHelper.CreateRandomList(100);
        
        var expected = list1
            .Join(list2, x => x, x => x, (x, y) => x ^ y, EqualityComparer<int>.Default)
            .Order()
            .ToList();
        
        var actual = await new TestEnumerable<int>(list1)
            .Join(new TestEnumerable<int>(list2), x => x, x => x, (x, y) => x ^ y, EqualityComparer<int>.Default)
            .Order()
            .ToListAsync();
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task TestNullElements() {
        var list1 = new[] { "first", "second", null, "oops" };
        var list2 = new[] { "oops", null, "third", "fourth" };
        
        var expected = list1
            .Join(list2, x => x, x => x, (x, y) => x + y)
            .Order()
            .ToList();
        
        var actual = await new TestEnumerable<string?>(list1)
            .Join(new TestEnumerable<string?>(list2), x => x, x => x, (x, y) => x + y)
            .Order()
            .ToListAsync();
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task TestEqualityComparer() {
        var list1 = TestHelper.CreateRandomList(100);
        var list2 = TestHelper.CreateRandomList(100);
        
        var expected = list1
            .Join(list2, x => x, x => x, (x, y) => x ^ y, new CustomComparer())
            .Order()
            .ToList();
        
        var actual = await new TestEnumerable<int>(list1)
            .Join(new TestEnumerable<int>(list2), x => x, x => x, (x, y) => x ^ y, new CustomComparer())
            .Order()
            .ToListAsync();
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task TestSelector() {
        var list1 = TestHelper.CreateRandomList(100);
        var list2 = TestHelper.CreateRandomList(100);
        
        var expected = list1
            .Join(list2, x => x % 10, x => x % 11, (x, y) => x ^ y)
            .Order()
            .ToList();
        
        var actual = await new TestEnumerable<int>(list1)
            .Join(new TestEnumerable<int>(list2), x => x % 10, x => x % 11, (x, y) => x ^ y)
            .Order()
            .ToListAsync();
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task TestConcurrency() {
        var list1 = TestHelper.CreateRandomList(100);
        var list2 = TestHelper.CreateRandomList(100);
        
        var expected = list1
            .Join(list2, x => x % 10, x => x % 11, (x, y) => x ^ y)
            .Order()
            .ToList();
        
        var actual = await list1.ToAsyncEnumerable()
            .AsConcurrent()
            .Join(list2.ToAsyncEnumerable(), x => x % 10, x => x % 11, (x, y) => x ^ y)
            .Order()
            .ToListAsync();
        
        Assert.Equal(expected, actual);
    }

    private class CustomComparer : IEqualityComparer<int> {
        public bool Equals(int x, int y) => x % 10 == y % 10;

        public int GetHashCode(int obj) => obj % 10;
    }
}