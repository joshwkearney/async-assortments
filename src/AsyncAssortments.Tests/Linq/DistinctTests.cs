using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class DistinctTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = (null as IAsyncEnumerable<int>)!;
        var seq = new TestEnumerable<int>([1, 2, 3]);

        // Sequence null checks

        Assert.Throws<ArgumentNullException>(() => nullSeq.Distinct());
        Assert.Throws<ArgumentNullException>(() => nullSeq.Distinct(EqualityComparer<int>.Default));

        Assert.Throws<ArgumentNullException>(() => nullSeq.DistinctBy(x => x));
        Assert.Throws<ArgumentNullException>(() => nullSeq.DistinctBy(x => x, EqualityComparer<int>.Default));
    }

    [Fact]
    public async Task TestExceptions() {        
        await Assert.ThrowsAsync<TestException>(async () => await TestHelper.GetFailingAsyncEnumerable().Distinct());
    }

    [Fact]
    public async Task TestEmpty() {
        var results1 = await AsyncEnumerable.Empty<int>().Distinct();
        var results2 = await AsyncEnumerable.Empty<int>().DistinctBy(x => x);

        Assert.Equal([], results1);
        Assert.Equal([], results2); 
    }

    [Fact]
    public async Task TestDefaultEquality() {
        var list = TestHelper.CreateRandomList(10);
        var seq = new TestEnumerable<int>(list);
        var expected = list.Distinct().ToList();

        var test1 = await seq.Distinct();
        var test2 = await seq.Distinct(EqualityComparer<int>.Default);
        var test3 = await seq.DistinctBy(x => x);
        var test4 = await seq.DistinctBy(x => x, EqualityComparer<int>.Default);

        Assert.Equal(expected, test1);
        Assert.Equal(expected, test2);
        Assert.Equal(expected, test3);
        Assert.Equal(expected, test4);
    }

    [Fact]
    public async Task TestCustomEquality() {
        var list = TestHelper.CreateRandomList(10);
        var seq = new TestEnumerable<int>(list);
        
        var expected = list.DistinctBy(x => x / 2);
        var test = await seq.DistinctBy(x => x / 2);

        Assert.Equal(expected, test);
    }
}