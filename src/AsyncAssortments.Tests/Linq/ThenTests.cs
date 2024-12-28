using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class ThenTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = (null as IAsyncEnumerable<int>)!;
        var seq = new TestEnumerable<int>([1, 2, 3]);

        // Sequence null checks

        Assert.Throws<ArgumentNullException>(() => nullSeq.Order().Then(Comparer<int>.Default));
        Assert.Throws<ArgumentNullException>(() => nullSeq.Order().ThenDescending(Comparer<int>.Default));
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.Order().ThenBy(x => x, Comparer<int>.Default));
        Assert.Throws<ArgumentNullException>(() => nullSeq.Order().ThenBy(x => x));
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.Order().ThenByDescending(x => x, Comparer<int>.Default));
        Assert.Throws<ArgumentNullException>(() => nullSeq.Order().ThenByDescending(x => x));

        // Comparer null checks

        Assert.Throws<ArgumentNullException>(() => seq.Order().Then(null!));
        Assert.Throws<ArgumentNullException>(() => seq.Order().ThenDescending(null!));
        
        Assert.Throws<ArgumentNullException>(() => seq.Order().ThenBy(x => x, null!));
        Assert.Throws<ArgumentNullException>(() => seq.Order().ThenByDescending(x => x, null!));

        // Lambda null checks
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.Order().ThenBy(null!, Comparer<int>.Default));
        Assert.Throws<ArgumentNullException>(() => nullSeq.Order().ThenBy<int, int>(null!));
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.Order().ThenByDescending(null!, Comparer<int>.Default));
        Assert.Throws<ArgumentNullException>(() => nullSeq.Order().ThenByDescending<int, int>(null!));
    }

    [Fact]
    public async Task TestExceptions() {        
        await Assert.ThrowsAsync<TestException>(async () => await TestHelper.GetFailingAsyncEnumerable().Order().ThenBy(x => x));
    }

    [Fact]
    public async Task TestEmpty() {
        var results1 = await AsyncEnumerable.Empty<int>().Order().Then(Comparer<int>.Default);
        var results2 = await AsyncEnumerable.Empty<int>().Order().ThenBy(x => -x);
        var results3 = await AsyncEnumerable.Empty<int>().Order().ThenDescending(Comparer<int>.Default);
        var results4 = await AsyncEnumerable.Empty<int>().Order().ThenByDescending(x => -x);

        Assert.Equal([], results1);
        Assert.Equal([], results2); 
        Assert.Equal([], results3); 
        Assert.Equal([], results4);
    }

    [Fact]
    public async Task TestAscending() {
        var list = TestHelper.CreateRandomList(50);
        var seq = new TestEnumerable<int>(list);
        var expected = list.OrderBy(x => x % 2).ThenBy(x => x).ToList();

        var test1 = await seq.OrderBy(x => x % 2).Then(Comparer<int>.Default);
        var test2 = await seq.OrderBy(x => x % 2).ThenBy(x => x);
        var test3 = await seq.OrderBy(x => x % 2).ThenBy(x => x, Comparer<int>.Default);

        Assert.Equal(expected, test1);
        Assert.Equal(expected, test2);
        Assert.Equal(expected, test3);
    }

    [Fact]
    public async Task TestDescending() {
        var list = TestHelper.CreateRandomList(50);
        var seq = new TestEnumerable<int>(list);
        var expected = list.OrderBy(x => x % 2).ThenByDescending(x => x).ToList();

        var test1 = await seq.OrderBy(x => x % 2).ThenDescending(Comparer<int>.Default);
        var test2 = await seq.OrderBy(x => x % 2).ThenByDescending(x => x);
        var test3 = await seq.OrderBy(x => x % 2).ThenByDescending(x => x, Comparer<int>.Default);

        Assert.Equal(expected, test1);
        Assert.Equal(expected, test2);
        Assert.Equal(expected, test3);
    }
    
    [Fact]
    public async Task TestMultipleSorts() {
        var list = TestHelper.CreateRandomList(50);
        var seq = new TestEnumerable<int>(list);
        var expected = list.OrderBy(x => x % 2).ThenBy(x => x % 3).ThenBy(x => x).ToList();

        var test1 = await seq.OrderBy(x => x % 2).ThenBy(x => x % 3).ThenBy(x => x);

        Assert.Equal(expected, test1);
    }
}