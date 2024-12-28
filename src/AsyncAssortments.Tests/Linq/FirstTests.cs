using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class FirstTests {
    [Fact]
    public void TestNullInputs() {
        var seq = new TestEnumerable<int>([1, 2, 3]);

        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().FirstAsync());
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().FirstAsync(x => x > 5));
        Assert.Throws<ArgumentNullException>(() => seq.FirstAsync(TestHelper.GetNullPredicate<int>()));
    }

    [Fact]
    public async Task TestExceptions() {        
        await Assert.ThrowsAsync<TestException>(async () => await TestHelper.GetFailingAsyncEnumerable().Where(x => x > int.MaxValue - 1).FirstAsync());
    }

    [Fact]
    public async Task TestEmpty() {
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await AsyncEnumerable.Empty<int>().FirstAsync());
    }

    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(10);
        var expected = list.First();
        var test = await new TestEnumerable<int>(list).FirstAsync();
        
        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestPredicate() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.First(x => x > 35);
        var test = await new TestEnumerable<int>(list).FirstAsync(x => x > 35);

        Assert.Equal(expected, test);
    }
}