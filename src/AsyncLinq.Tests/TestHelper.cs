using System.Diagnostics;

namespace AsyncCollections.Linq.Tests;

public static class TestHelper {
    public static List<int> CreateRandomList(int size, int seed = 1234) {
        var list = new List<int>(size);
        var rand = new Random(seed);

        for (int i = 0; i < size; i++) {
            list.Add(rand.Next(0, 100));
        }

        return list;
    }

    public static async Task<TimeResult<T>> TimeAsync<T>(Func<Task<T>> action) {
        await action();

        var watch = new Stopwatch();
        watch.Start();

        var result = await action();

        return new TimeResult<T>(watch.ElapsedMilliseconds, result);
    }
}

public readonly record struct TimeResult<T>(long ElapsedMilliseconds, T Value) {
}

public class TestException : Exception {
    private static int id = 0;
        
    public TestException() : base("This is an exception meant for unit testing with id = " + id++) { }
}

public class TestEnumerable<T> : IAsyncEnumerable<T> {
    private readonly IReadOnlyList<T> list;

    public TestEnumerable(IReadOnlyList<T> list) {
        this.list = list;
    }
    
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
        foreach (var item in this.list) {
            await Task.Delay(10, cancellationToken);

            yield return item;
        }
    }
}