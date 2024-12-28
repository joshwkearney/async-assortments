using System.Reactive.Linq;
using AsyncAssortments.Linq;

namespace AsyncLinq.Tests.Linq;

public class ToObservableTests {
    [Fact]
    public void TestNullInputs() {
        Assert.Throws<ArgumentNullException>(() => TestHelper.GetNullAsyncEnumerable().ToObservable());
    }
    
    [Fact]
    public async Task TestException() {
        var seq = new TestEnumerable<int>([1, 2, 3]).Select<int, int>(x => throw new TestException());
        
        await Assert.ThrowsAsync<TestException>(async () => await seq.ToObservable().ToList());
    }
    
    [Fact]
    public async Task TestSequence() {
        var seq = new TestEnumerable<int>([1, 2, 3]);
        var elements = await seq.ToObservable().ToList();
        
        Assert.Equal([1, 2, 3], elements);
    }
    
    [Fact]
    public async Task TestEmptySequence() {
        var elements = await AsyncEnumerable.Empty<int>().ToObservable().ToList();
        
        Assert.Equal([], elements);
    }
}