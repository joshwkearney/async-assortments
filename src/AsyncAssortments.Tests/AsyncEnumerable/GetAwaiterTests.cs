using Microsoft.Win32.SafeHandles;

namespace AsyncAssortments.Linq.Tests;

public class GetAwaiterTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;
        
        Assert.Throws<ArgumentNullException>(() => nullSeq.GetAwaiter());
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