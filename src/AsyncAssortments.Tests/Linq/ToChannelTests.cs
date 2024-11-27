using System.Reactive.Linq;
using AsyncAssortments.Linq;

namespace AsyncAssortments.Linq.Tests;

public class ToChannelTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;

        Assert.Throws<ArgumentNullException>(() => nullSeq.ToChannel());
    }
    
    [Fact]
    public async Task TestException() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Select<int, int>(x => throw new TestException());
        var count = 0;

        await Assert.ThrowsAsync<TestException>(async () => {
            await foreach (var item in seq.ToChannel().ReadAllAsync()) {
                count++;
            }
        });
    }
    
    [Fact]
    public async Task TestSequence() {
        var seq = new TestEnumerable<int>([1, 2, 3]);
        var elements = await seq.ToChannel().ReadAllAsync();
        
        Assert.Equal([1, 2, 3], elements);
    }
    
    [Fact]
    public async Task TestEmptySequence() {
        var elements = await AsyncEnumerable.Empty<int>().ToChannel().ReadAllAsync();
        
        Assert.Equal([], elements);
    }
}