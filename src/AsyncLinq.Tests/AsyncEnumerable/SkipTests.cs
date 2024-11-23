using AsyncCollections.Linq;

namespace AsyncCollections.Linq.Tests;

public class SkipTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.Skip(10));
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