namespace AsyncLinq.Tests;

public class LastTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        var seq = new TestEnumerable<int>([1, 2, 3]);

        Assert.Throws<ArgumentNullException>(() => nullSeq.LastAsync());
        Assert.Throws<ArgumentNullException>(() => nullSeq.LastAsync(x => x > 5));

        Assert.Throws<ArgumentNullException>(() => seq.LastAsync(null));
    }

    [Fact]
    public async Task TestExceptions() {        
        await Assert.ThrowsAsync<TestException>(async () => await GetBad().LastAsync());

        async IAsyncEnumerable<int> GetBad() {
            throw new TestException();
            yield return 10;
        }
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