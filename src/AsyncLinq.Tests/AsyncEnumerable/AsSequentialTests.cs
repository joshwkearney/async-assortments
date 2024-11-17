using System.Diagnostics;

namespace AsyncLinq.Tests;

public class AsSequentialTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;

        Assert.Throws<ArgumentNullException>(() => nullSeq.AsSequential());
    }
}