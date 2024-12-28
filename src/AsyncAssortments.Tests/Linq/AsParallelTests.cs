using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class AsParallelTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsParallel(true));
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsParallel(false));
    }
}