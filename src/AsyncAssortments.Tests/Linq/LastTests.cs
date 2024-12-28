using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class LastTests {
    [Fact]
    public void TestNullInputs() {
        var seq = new TestEnumerable<int>([1, 2, 3]);

        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().LastAsync());
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().LastAsync(x => x > 5));

        Assert.Throws<ArgumentNullException>(() => seq.LastAsync(null!));
    }

    [Fact]
    public async Task TestExceptions() {        
        await Assert.ThrowsAsync<TestException>(async () => await TestHelper.GetFailingAsyncEnumerable().LastAsync());
    }

    [Fact]
    public async Task TestEmpty() {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.Empty<int>().LastAsync());
    }

    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(10);
        var expected = list.Last();
        var test = await new TestEnumerable<int>(list).LastAsync();
        
        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestPredicate() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Last(x => x > 35);
        var test = await new TestEnumerable<int>(list).LastAsync(x => x > 35);

        Assert.Equal(expected, test);
    }
}