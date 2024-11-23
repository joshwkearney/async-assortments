using AsyncCollections.Linq;

namespace AsyncCollections.Linq.Tests;

public class LastOrDefaultTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        var seq = new TestEnumerable<int>([1, 2, 3]);

        Assert.Throws<ArgumentNullException>(() => nullSeq.LastOrDefaultAsync());
        Assert.Throws<ArgumentNullException>(() => nullSeq.LastOrDefaultAsync(x => x > 5));

        Assert.Throws<ArgumentNullException>(() => seq.LastOrDefaultAsync(null));
    }

    [Fact]
    public async Task TestExceptions() {        
        await Assert.ThrowsAsync<TestException>(async () => await GetBad().LastOrDefaultAsync());
        await Assert.ThrowsAsync<TestException>(async () => await GetBad().LastOrDefaultAsync(x => x > 5));
        await Assert.ThrowsAsync<TestException>(async () => await GetBad().LastOrDefaultAsync(x => x > 78, 99));

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield return 10;
        }
    }

    [Fact]
    public async Task TestEmpty1() {
        var expected = default(int);
        var actual = await AsyncEnumerable.Empty<int>().LastOrDefaultAsync();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestEmpty2() {
        var expected = default(string);
        var actual = await AsyncEnumerable.Empty<string>().LastOrDefaultAsync();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestEmpty3() {
        var expected = 99;
        var actual = await AsyncEnumerable.Empty<int>().LastOrDefaultAsync(99);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestEmpty4() {
        var expected = "thing";
        var actual = await AsyncEnumerable.Empty<string>().LastOrDefaultAsync("thing");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestEmpty5() {
        var expected = 99;
        var actual = await AsyncEnumerable.Empty<int>().LastOrDefaultAsync(x => x > 3, 99);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestEmpty6() {
        var expected = "thing";
        var actual = await AsyncEnumerable.Empty<string>().LastOrDefaultAsync(x => x.Length > 2, "thing");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(10);
        var expected = list.LastOrDefault();
        var test = await new TestEnumerable<int>(list).LastOrDefaultAsync();
        
        Assert.Equal(expected, test);
    }

    [Fact]
    public async Task TestPredicate() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.LastOrDefault(x => x > 35);
        var test = await new TestEnumerable<int>(list).LastOrDefaultAsync(x => x > 35);

        Assert.Equal(expected, test);
    }
}