# Async Collections

Hey look! It's LINQ operators for `IAsyncEnumerable`!

```csharp
var ids = [1, 2, 3 /* etc */];

var result = await ids
    .AsAsyncEnumerable()
    .AsParallel()
    .AsyncSelect(async x => await ProcessItem(x))
    .Where(x => x.Florbs.Count < 10)
    .SelectMany(x => x.Florbs)
    .ToListAsync();
```

## What is an `IAsyncEnumerable<T>` anyway?

.NET has added the `IAsyncEnumerable` interface, but what kind of collection does this actually represent? The 
standard answer
is a sequence of elements with a potentially long delay in retrieving the next item,
and hence the `Async` in the name. This model works for many uses cases (streaming a file, database queries, etc) but
runs into issues when interacting with the rest of the `Task<T>` ecosystem.

Tasks in C# are used for long-delayed operations, but they are also used for queuing work to either run
concurrently (at the same time) or in parallel (separate processors). A very common feature of codebases I've
seen is an `IEnumerable<Task<T>>` to represent parallel work, and at first this seems like a no-brainer to
convert to `IAsyncEnumerable<T>`. It's an enumerable that has to be awaited right? In theory yes, you could use
the exact same interface to represent the work being completed by a list of tasks.

The way I see it, an `IAsyncEnumerable<T>` could represent three different kinds of sequences:

1. A sequence of elements where there is a potentially long delay in retrieving the next object
2. A collection of work running concurrently, where the results that finish first are yielded first
3. A collection of work running in parallel, where the tasks that finish first are yielded first

Think of how an `IEnumerable<T>` can represent a list, a set, or a dictionary. Same interface,
different underlying semantics.

Unfortunately, other operators for `IAsyncEnumerable`, like those added, by RX.NET, do not support enumerables that
represent parallel work like an `IEnumerable<Task<T>>` does.

## Where does this library come in?
This library provides an implementation of these operators that work for all three use cases. Like RX.NET,
sequential enumerables are the default here too:

```csharp
var items = await new[] { 300, 200, 100 }
    .AsAsyncEnumerable()
    .AsyncSelect(async x => { await Task.Delay(x); return x; })
    .ToListAsync();
```

This will take 600ms to run because the task delays are executed sequentially while enumerating the collection. 
However, if we wanted to change this so that the select tasks run at the same time, it's very easy:

```csharp
var items = await new[] { 300, 200, 100 }
    .AsAsyncEnumerable()
    .AsConcurrent()
    .AsyncSelect(async x => { await Task.Delay(x); return x; })
    .ToListAsync();
```

Nice! One extra line and now it finishes in 300ms. But what if we were only interested in the first item this
produces?

```csharp
var first = await new[] { 300, 200, 100 }
    .AsAsyncEnumerable()
    .AsConcurrent()
    .AsyncSelect(async x => { await Task.Delay(x); return x; })
    .FirstAsync();
```

Now this will give us one element, and it only takes 100ms to run. Perfect! When running concurrently, `AsyncSelect` 
returns items in the order they finish, not necessarily in the original order.

This is much more powerful than a naive `IEnumerable<Task<T>>` because
there are no barriers between transformation steps: completed items are yielded immediately
to the next operation rather than waiting on the entire transformation to finish. This is great for
complicated multi-step pipelines where good parallel performance is important.

This example uses `AsConcurrent()` to allow the selectors to run at the same time, but there is a difference
between `AsConcurrent()` and `AsParallel()`. Concurrent enumerables allow operations to run at the same
time but never queue work on the threadpool; all the work still happens on one thread. Parallel enumerables
use `Parallel.ForEachAsync()` so the work will be distributed across multiple processing cores. For IO-bound
operations use `AsConcurrent()`, for CPU-bound operations use `AsParallel()`.