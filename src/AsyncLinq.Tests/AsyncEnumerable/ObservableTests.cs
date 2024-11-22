using System.Reactive.Linq;

namespace AsyncLinq.Tests;

public class ObservableTests {
    [Fact]
    public void TestNullInputs() {
        var nullSeq = null as IAsyncEnumerable<int>;

        Assert.Throws<ArgumentNullException>(() => nullSeq.ToObservable());
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