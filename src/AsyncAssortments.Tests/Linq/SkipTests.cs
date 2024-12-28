using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class SkipTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().Skip(10));
    }

    [Fact]
    public async Task TestNegative() {
        var seq = TestHelper.CreateRandomList(100);
        var test = await new TestEnumerable<int>(seq).Skip(-500).ToListAsync();

        Assert.Equal(seq, test);
    }

    [Fact]
    public async Task TestEmpty() {
        var seq = TestHelper.CreateRandomList(100);
        var test = await new TestEnumerable<int>(seq).Skip(0).ToListAsync();

        Assert.Equal(seq, test);
    }

    [Fact]
    public async Task TestRandomSequence() {
        var list = TestHelper.CreateRandomList(100);
        var expected = list.Skip(16).ToArray();

        var test = await new TestEnumerable<int>(list)
            .Skip(16)
            .ToListAsync();

        Assert.Equal(expected, test);
    }
}