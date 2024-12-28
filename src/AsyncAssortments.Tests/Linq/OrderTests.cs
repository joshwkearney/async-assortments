using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class OrderTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = (null as IAsyncEnumerable<int>)!;
        var seq = new TestEnumerable<int>([1, 2, 3]);

        // Sequence null checks

        Assert.Throws<ArgumentNullException>(() => nullSeq.Order());
        Assert.Throws<ArgumentNullException>(() => nullSeq.Order(Comparer<int>.Default));

        Assert.Throws<ArgumentNullException>(() => nullSeq.OrderDescending());
        Assert.Throws<ArgumentNullException>(() => nullSeq.OrderDescending(Comparer<int>.Default));

        Assert.Throws<ArgumentNullException>(() => nullSeq.OrderBy(x => x));
        Assert.Throws<ArgumentNullException>(() => nullSeq.OrderBy(x => x, Comparer<int>.Default));

        Assert.Throws<ArgumentNullException>(() => nullSeq.OrderByDescending(x => x));
        Assert.Throws<ArgumentNullException>(() => nullSeq.OrderByDescending(x => x, Comparer<int>.Default));

        // Comparer null checks

        Assert.Throws<ArgumentNullException>(() => seq.Order(null!));
        Assert.Throws<ArgumentNullException>(() => seq.OrderDescending(null!));

        Assert.Throws<ArgumentNullException>(() => seq.OrderBy(x => x, null!));
        Assert.Throws<ArgumentNullException>(() => seq.OrderByDescending(x => x, null!));

        // Lambda null checks

        Assert.Throws<ArgumentNullException>(() => seq.OrderBy<int, int>(null!));
        Assert.Throws<ArgumentNullException>(() => seq.OrderBy(null!, Comparer<int>.Default));

        Assert.Throws<ArgumentNullException>(() => seq.OrderByDescending<int, int>(null!));
        Assert.Throws<ArgumentNullException>(() => seq.OrderByDescending(null!, Comparer<int>.Default));
    }

    [Fact]
    public async Task TestExceptions() {        
        await Assert.ThrowsAsync<TestException>(async () => await TestHelper.GetFailingAsyncEnumerable().Order());
        await Assert.ThrowsAsync<TestException>(async () => await TestHelper.GetFailingAsyncEnumerable().OrderBy(x => x));
    }

    [Fact]
    public async Task TestEmpty() {
        var results1 = await AsyncEnumerable.Empty<int>().Order();
        var results2 = await AsyncEnumerable.Empty<int>().OrderBy(x => x);
        var results3 = await AsyncEnumerable.Empty<int>().OrderDescending();
        var results4 = await AsyncEnumerable.Empty<int>().OrderByDescending(x => x);

        Assert.Equal([], results1);
        Assert.Equal([], results2); 
        Assert.Equal([], results3); 
        Assert.Equal([], results4);
    }

    [Fact]
    public async Task TestAscending() {
        var list = TestHelper.CreateRandomList(10);
        var seq = new TestEnumerable<int>(list);
        var expected = list.Order();

        var test1 = await seq.Order();
        var test2 = await seq.Order(Comparer<int>.Default);
        var test3 = await seq.OrderBy(x => x);
        var test4 = await seq.OrderBy(x => x, Comparer<int>.Default);

        Assert.Equal(expected, test1);
        Assert.Equal(expected, test2);
        Assert.Equal(expected, test3);
        Assert.Equal(expected, test4);
    }

    [Fact]
    public async Task TestDescending() {
        var list = TestHelper.CreateRandomList(10);
        var seq = new TestEnumerable<int>(list);
        var expected = list.OrderDescending();

        var test1 = await seq.OrderDescending();
        var test2 = await seq.OrderDescending(Comparer<int>.Default);
        var test3 = await seq.OrderByDescending(x => x);
        var test4 = await seq.OrderByDescending(x => x, Comparer<int>.Default);

        Assert.Equal(expected, test1);
        Assert.Equal(expected, test2);
        Assert.Equal(expected, test3);
        Assert.Equal(expected, test4);
    }

    [Fact]
    public async Task TestOrderByAscending() {
        var list = TestHelper.CreateRandomList(10);
        var seq = new TestEnumerable<int>(list);
        var expected = list.OrderBy(x => (int)Math.Sqrt(x));

        var test3 = await seq.OrderBy(x => (int)Math.Sqrt(x));
        var test4 = await seq.OrderBy(x => (int)Math.Sqrt(x), Comparer<int>.Default);

        Assert.Equal(expected, test3);
        Assert.Equal(expected, test4);
    }

    [Fact]
    public async Task TestOrderByDescending() {
        var list = TestHelper.CreateRandomList(10);
        var seq = new TestEnumerable<int>(list);
        var expected = list.OrderByDescending(x => (int)Math.Sqrt(x));

        var test3 = await seq.OrderByDescending(x => (int)Math.Sqrt(x));
        var test4 = await seq.OrderByDescending(x => (int)Math.Sqrt(x), Comparer<int>.Default);

        Assert.Equal(expected, test3);
        Assert.Equal(expected, test4);
    }
}