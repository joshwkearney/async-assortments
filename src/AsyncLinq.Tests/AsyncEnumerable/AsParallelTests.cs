using AsyncCollections.Linq;

namespace AsyncCollections.Linq.Tests;

public class AsParallelTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;

        Assert.Throws<ArgumentNullException>(() => nullSeq.AsParallel(true));
        Assert.Throws<ArgumentNullException>(() => nullSeq.AsParallel(false));
    }
}