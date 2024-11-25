using AsyncCollections.Linq;

namespace AsyncCollections.Linq.Tests;

public class AsConcurrentTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;

        Assert.Throws<ArgumentNullException>(() => nullSeq.AsConcurrent(true));
        Assert.Throws<ArgumentNullException>(() => nullSeq.AsConcurrent(false));
    }      
}