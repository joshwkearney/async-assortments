# AsyncCollections.Linq

Hey look! It's async LINQ operators for `IAsyncEnumerable` !

```csharp
var ids = new int[] { 1, 2, 3 /* etc */ };

var result = await ids
    .ToAsyncEnumerable()
    .Where(x => x >= 100)
    .AsyncSelect(async x => await ProcessItem(x))
    .Take(20)
    .ToListAsync();
```

## Download
The latest version of `AsyncCollections.Linq` can be downloaded on NuGet 
[here](https://www.nuget.org/packages/AsyncCollections.Linq/).

## Purpose
Microsoft has made it clear that they have 
[no intentions](https://github.com/dotnet/runtime/issues/31580#issuecomment-581065904) 
of shipping LINQ extensions for `IAsyncEnumerable` and they 
[don't think all useful functionality must be included in the standard 
library](https://github.com/dotnet/runtime/issues/31580#issuecomment-636364261). Well, 
challenge accepted! This project aims to be a high-quality implementation of
LINQ for `IAsyncEnumerable`.

## Why not use `System.Reactive.Linq` ?
The great [RX.NET project](https://github.com/dotnet/reactive) maintains the 
`System.Linq.Async` namespace, which contains LINQ operators for `IAsyncEnumerable`. Why
create a new project that aims to do the same thing?

RX.NET always assumes an `IAsyncEnumerable` represents a linear sequence of 
elements with a long time delay between each element. This is fine for something like a 
database query (or a reactive stream), but this wasn't my use case. 

Instead, I was trying to find an abstraction that could represent a collection of 
tasks all executing *simultaneously*, not just one at a time. I realized that 
`IAsyncEnumerable` was actually the perfect interface for this, but it would require 
LINQ operators that could compose correctly under concurrent and parallel execution. 

RX.NET does not do this, so a new library was born. Keep reading for details!

## Async pipelines?

I'm sure most people reading this have run into the following situation: you have a
list of things and want to apply a function to each element. Normally, you would
call `.Select()` and be on your way, but this time is tricky because your function
return a `Task<T>`. You could write a `foreach` loop and await the function inside,
but that's going to take much longer than if you start all the tasks 
and then await all of them. Usually you end up with something like this:

```csharp
var ids = new int[] { 1, 2, 3, 4 };
var tasks = ids.Select(async x => await ProcessItem(x));
var results = await Task.WhenAll(tasks);
```

Ok, that's not so bad. But what if you want to filter the results afterwards, but your
filtering function also returns a `Task<bool>`:

```csharp
var ids = new int[] { 1, 2, 3, 4 };
var tasks = ids.Select(async x => await ProcessItem(x));
var results = await Task.WhenAll(tasks);
var tasks2 = results.Select(async x => new { IsValid = await FilterItem(x), Item = x });
var temp = await Task.WhenAll(tasks2);
var results2 = temp.Where(x => x.IsValid).Select(x => x.Item).ToList();
```

Oh dear, that's getting pretty terrible. If we need more operations this will 
turn into an absolute mess. This also isn't a very good solution, 
because all of our first tasks have to finish before any of the next tasks start, which 
means we're wasting time when we could be getting the results faster.

Surely there's a better way to build async pipelines like this?

## Async pipelines!

Instead, you can use this library to write the code above as follows:

```csharp
var results = await new[] { 1, 2, 3, 4 }
    .ToAsyncEnumerable()
    .AsConcurrent(preserveOrder: false)
    .AsyncSelect(async x => await ProcessItem(x))
    .AsyncWhere(async x => await FilterItem(x))
    .ToListAsync();
```

Now, that's much better! It's easier to read, easier to modify, extremely clear, and it's
faster and will yield results sooner. 

By calling `.AsConcurrent()` we're instructing `AsyncSelect` and `AsyncWhere` to run
their selector functions concurrently, so all the tasks are all started before awaiting
just like when using `await Task.WhenAll()` above. 

The argument `preserveOrder: false`
also tells the pipeline that results don't have to be returned in the same order 
as the original sequence, meaning as the tasks that finish they will immediately
move to the next step. Without this, the tasks will still execute concurrently but
the results will always be returned in the original order, which could decrease
throughput.

Finally we apply our transformations using the async versions of the usual
LINQ operators that return tasks, collect our results in a list, and we're done!