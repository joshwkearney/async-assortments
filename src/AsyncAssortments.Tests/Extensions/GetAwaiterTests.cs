using AsyncAssortments;
using AsyncAssortments.Linq;
using Microsoft.Win32.SafeHandles;

namespace AsyncLinq.Tests.Extensions;

public class GetAwaiterTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().GetAwaiter());
    }

    [Fact]
    public async Task TestRandomSequence1() {
        var list = TestHelper.CreateRandomList(100);
        var seq = new TestEnumerable<int>(list) as IAsyncEnumerable<int>;
        var test = await seq;

        Assert.Equal(list, test);
    }

    [Fact]
    public async Task TestRandomSequence2() {
        var list = TestHelper.CreateRandomList(100);
        var seq = list.ToAsyncEnumerable();
        var test = await seq;

        Assert.Equal(list, test);
    }

    [Fact]
    public async Task TestRandomSequence3() {
        var list = TestHelper.CreateRandomList(10);

        var seq = list
            .ToAsyncEnumerable()
            .AsyncSelect(async x => {
                await Task.Delay(20);
                return x;
            });

        var test = await seq;

        Assert.Equal(list, test);
    }
}