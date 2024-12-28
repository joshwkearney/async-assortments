using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class FirstOrDefaultTests {
    [Fact]
    public void TestNullInputs() {
        var seq = new TestEnumerable<int>([1, 2, 3]);

        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().FirstOrDefaultAsync());
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().FirstOrDefaultAsync(x => x > 5));
        Assert.Throws<ArgumentNullException>(() => seq.FirstOrDefaultAsync(TestHelper.GetNullPredicate<int>()));
    }

    [Fact]
    public async Task TestExceptions() {        
        await Assert.ThrowsAsync<TestException>(async () => await TestHelper.GetFailingAsyncEnumerable().Where(x => x > int.MaxValue - 1).FirstOrDefaultAsync());
        await Assert.ThrowsAsync<TestException>(async () => await TestHelper.GetFailingAsyncEnumerable().FirstOrDefaultAsync(x => x > int.MaxValue - 1));
        await Assert.ThrowsAsync<TestException>(async () => await TestHelper.GetFailingAsyncEnumerable().FirstOrDefaultAsync(x => x > int.MaxValue - 1, 99));
    }

    [Fact]
    public async Task TestEmpty1() {
        var expected = default(int);
        var actual = await AsyncEnumerable.Empty<int>().FirstOrDefaultAsync();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestEmpty2() {
        var expected = default(string);
        var actual = await AsyncEnumerable.Empty<string>().FirstOrDefaultAsync();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestEmpty3() {
        var expected = 99;
        var actual = await AsyncEnumerable.Empty<int>().FirstOrDefaultAsync(99);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestEmpty4() {
        var expected = "thing";
        var actual = await AsyncEnumerable.Empty<string>().FirstOrDefaultAsync("thing");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestEmpty5() {
        var expected = 99;
        var actual = await AsyncEnumerable.Empty<int>().FirstOrDefaultAsync(x => x > 3, 99);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestEmpty6() {
        var expected = "thing";
        var actual = await AsyncEnumerable.Empty<string>().FirstOrDefaultAsync(x => x.Length > 2, "thing");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(10);
        var expected = list.FirstOrDefault();
        var test = await new TestEnumerable<int>(list).FirstOrDefaultAsync();
        
        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestPredicate() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.FirstOrDefault(x => x > 35);
        var test = await new TestEnumerable<int>(list).FirstOrDefaultAsync(x => x > 35);

        Assert.Equal(expected, test);
    }
}