using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class ExceptTests {
    [Fact]
    public void TestNullInputs() {
        var seq1 = new TestEnumerable<int>([1, 2, 3]);
        var seq2 = new TestEnumerable<int>([1, 2, 3]);
        
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().Except(seq2));
        Assert.Throws<ArgumentNullException>(() => seq1.Except(TestHelper.GetNullAsyncEnumerable()));
        Assert.Throws<ArgumentNullException>(() => seq1.Except(seq2, null!));
    }
    
    [Fact]
    public async Task TestExceptions1() {
        var seq1 = TestHelper.GetFailingAsyncEnumerable();
        var seq2 = new TestEnumerable<int>([1, 2, 3]);
        
        await Assert.ThrowsAsync<TestException>(async () => await seq1.Except(seq2).ToListAsync());
    }
    
    [Fact]
    public async Task TestExceptions2() {
        var seq1 = TestHelper.GetFailingAsyncEnumerable();
        var seq2 = new TestEnumerable<int>([1, 2, 3]);
        
        await Assert.ThrowsAsync<TestException>(async () => await seq2.Except(seq1).ToListAsync());
    }
    
    [Fact]
    public async Task TestRandomSequences() {
        var list1 = TestHelper.CreateRandomList(100);
        var list2 = TestHelper.CreateRandomList(50);
        
        var expected = list1
            .Except(list2)
            .Order()
            .ToList();
        
        var actual = await new TestEnumerable<int>(list1)
            .Except(new TestEnumerable<int>(list2))
            .Order()
            .ToListAsync();
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task TestNullElements() {
        var list1 = new[] { "first", "second", null, "oops" };
        var list2 = new[] { "oops", null, "third", "fourth" };
        
        var expected = list1
            .Except(list2)
            .Order()
            .ToList();
        
        var actual = await new TestEnumerable<string?>(list1)
            .Except(new TestEnumerable<string?>(list2))
            .Order()
            .ToListAsync();
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task TestEqualityComparer() {
        var list1 = TestHelper.CreateRandomList(100);
        var list2 = TestHelper.CreateRandomList(50);
        
        var expected = list1
            .ExceptBy(list2, x => x % 11)
            .Order()
            .ToList();
        
        var actual = await new TestEnumerable<int>(list1)
            .ExceptBy(new TestEnumerable<int>(list2), x => x % 11)
            .Order()
            .ToListAsync();
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task TestEqualityComparer2() {
        var list1 = new List<int>() { 1, 2, 3 };
        var list2 = new List<int>() { 1, 2, 3 };
        
        var expected = list1
            .ExceptBy(list2, x => x % 3)
            .Order()
            .ToList();
        
        var actual = await new TestEnumerable<int>(list1)
            .ExceptBy(new TestEnumerable<int>(list2), x => x % 3)
            .Order()
            .ToListAsync();
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public async Task TestConcurrency() {
        var list1 = TestHelper.CreateRandomList(100);
        var list2 = TestHelper.CreateRandomList(50);
        
        var expected = list1
            .Except(list2)
            .Order()
            .ToList();
        
        var actual = await list1.ToAsyncEnumerable()
            .AsConcurrent()
            .Except(list2.ToAsyncEnumerable())
            .Order()
            .ToListAsync();
        
        Assert.Equal(expected, actual);
    }
}