using AsyncAssortments;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class ToChannelTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().ToChannel());
    }
    
    [Fact]
    public async Task TestException() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Select<int, int>(x => throw new TestException());

        await Assert.ThrowsAsync<TestException>(async () => {
            await foreach (var _ in seq.ToChannel().Reader.ReadAllAsync()) {
                await Task.Delay(1);
            }
        });
    }
    
    [Fact]
    public async Task TestSequence() {
        var seq = new TestEnumerable<int>([1, 2, 3]);
        var elements = await seq.ToChannel().Reader.ReadAllAsync();
        
        Assert.Equal([1, 2, 3], elements);
    }
    
    [Fact]
    public async Task TestEmptySequence() {
        var elements = await AsyncEnumerable.Empty<int>().ToChannel().Reader.ReadAllAsync();
        
        Assert.Equal([], elements);
    }
}