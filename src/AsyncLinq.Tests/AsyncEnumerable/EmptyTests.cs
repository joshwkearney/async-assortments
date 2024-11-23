using AsyncCollections.Linq;

namespace AsyncCollections.Linq.Tests;

public class EmptyTests {    
    [Fact]
    public async Task TestNoElements() {
        var count = await AsyncEnumerable.Empty<int>().CountAsync();
        var any = await AsyncEnumerable.Empty<int>().AnyAsync();
        
        Assert.Equal(0, count);
        Assert.False(any);
    }
}