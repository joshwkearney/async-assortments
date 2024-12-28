using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class AsSequentialTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsSequential());
    }
}