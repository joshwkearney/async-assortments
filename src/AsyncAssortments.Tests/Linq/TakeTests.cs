using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class TakeTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().Take(10));
    }

    [Fact]
    public async Task TestNegative() {
        var seq = TestHelper.CreateRandomList(100);
        var test = await new TestEnumerable<int>(seq).Take(-500).ToListAsync();

        Assert.Equal([], test);
    }

    [Fact]
    public async Task TestEmpty() {
        var seq = TestHelper.CreateRandomList(100);
        var test = await new TestEnumerable<int>(seq).Take(0).ToListAsync();

        Assert.Equal([], test);
    }

    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Take(57).ToArray();

        var test = await new TestEnumerable<int>(list)
            .Take(57)
            .ToListAsync();

        Assert.Equal(expected, test);
    }
}