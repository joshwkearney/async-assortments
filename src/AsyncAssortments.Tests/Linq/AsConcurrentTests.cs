using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class AsConcurrentTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsConcurrent(true));
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().AsConcurrent(false));
    }      
}