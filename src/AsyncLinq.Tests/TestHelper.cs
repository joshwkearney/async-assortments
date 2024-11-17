namespace AsyncLinq.Tests;

public static class TestHelper {
    public static List<int> CreateRandomList(int size, int seed = 1234) {
        var list = new List<int>(size);
        var rand = new Random(seed);

        for (int i = 0; i < size; i++) {
            list.Add(rand.Next(0, 100));
        }

        return list;
    }
}

public class TestException : Exception {
    private static int id = 0;
        
    public TestException() : base("This is an exception meant for unit testing with id = " + id++) { }
}

public class TestEnumerable<T> : IAsyncEnumerable<T> {
    private readonly List<T> list;

    public TestEnumerable(List<T> list) {
        this.list = list;
    }
    
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
        foreach (var item in this.list) {
            await Task.Delay(10, cancellationToken);

            yield return item;
        }
    }
}