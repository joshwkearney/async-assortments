# Async Linq

Hey look! It's LINQ operators for `IAsyncEnumerable` !

```csharp
var ids = new int[] { 1, 2, 3 /* etc */ };

var result = await ids
    .ToAsyncEnumerable()
    .Where(x => x <= 2)
    .Select(x => x * 2)
    .SelectMany(x => new[] { x, x })
    .ToListAsync();
```

## Download
The latest version of AsyncLinq can be downloaded on NuGet [here](https://example.com).

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

In short, the operator implementations in that library did not match my use case for
`IAsyncEnumerable`. In that implementation, an `IAsyncEnumerable` is always treated
as a linear sequence of elements, and the async operators like `SelectAwait` are applied
one at a time while the sequence is being enumerated.

The issue with this was pointed out very well by the user CodingOctocat 
[here](https://github.com/dotnet/runtime/issues/29145#issuecomment-495878382):

> If I have a IAsyncEnumerable that contains 20 elements, and each element takes 1 second to 
> get, and if I just want to get the last element, then it takes 20 seconds for me to use 
> Last()?
>
> This is crazy!

What if, instead of representing a linear sequence, you could use `IAsyncEnumerable` to 
represent a set of *concurrent* operations that all run at the same time? What if you
could have a sequence, call `.AsyncSelect()`, and have all of the selectors run 
at the same time? Almost like creating a pipeline with calls to `Task.WhenAll`? 

This was my use case, and this library implements the async operators so that they can
run sequentially, like `System.Linq.Async`, concurrently, or in parallel. Keep reading
for details! 

## What is an `IAsyncEnumerable<T>` anyway?
.NET added support for the `IAsyncEnumerable` interface and `await foreach` loops, but an 
interesting question is what kind of collection does this interface represent? We know that
the normal `IEnumerable` interface can represent many different types of collections, so 
wouldn't it be reasonable to guess that `IAsyncEnumerable` could represent different
kinds of *async* collections? If so what are they?

The one obvious answer is that an `IAsyncEnumerable` could be a sequence of items with a
potentially very long delay between elements, like if you're fetching records from a
database. This is what RX.NET assumes, and it's probably the most common use case of
`IAsyncEnumerable`.

However, have you ever found yourself writing code like this?

```csharp
var ids = new int[] { 1, 2, 3, 4 };
var tasks = ids.Select(async x => await ProcessItem(x));
var results = await Task.WhenAll(tasks);
```

Imagine we have a list of ids, and we want to do some processing on each id. The
processing involves a database query so it returns a `Task<T>`, and we want to start the
tasks at the same time so all the transactions happen at once. 

Now say we want to filter the results after processing, but our filter also returns a 
`Task<bool>` (alas we're calling a microservice). Well, add this:

```csharp
var tasks2 = results.Select(async x => new { IsValid = await FilterItem(x), Item = x });
var temp = await Task.WhenAll(tasks2);
var results2 = temp.Where(x => x.IsValid).Select(x => x.Item).ToList();
```

This is going to get tedious rather quickly if we have too many processing steps. This 
solution also isn't even a good one, because we can't even start the `.Where()` tasks
until all of the `.Select()` tasks are done. What if some of our tasks finish instantly
and some take 10 seconds? Too bad, we have to wait for all of them. 

Might it be possible to represent our `IEnumerable<Task<T>>` as an `IAsyncEnumerable<T>`?
If so then we could just use LINQ operators on the `IAsyncEnumerable` and crunch this code
down to a few lines. Hmmm

## AsyncLinq to the rescue!
You can rewrite the code above with AsyncLinq like this:

```csharp
var results = new[] { 1, 2, 3, 4 }
    .ToAsyncEnumerable()
    .AsConcurrent(preserveOrder: false)
    .AsyncSelect(async x => await ProcessItem(x))
    .AsyncWhere(async x => await FilterItem(x))
    .ToListAsync();
```

No problem! `AsConcurrent` instructs the following operators to start their tasks at the
same time, and then we can just apply `AsyncSelect` and `AsyncWhere` to transform our data.
Even better, we told `AsConcurrent` that it doesn't have to preserve the order of the
sequence, so as items finish one processing step they can immediately be yielded to the next
without waiting on the previous items in the sequence. 

This is far easier to read than before, it's more efficient, and it's extremely simple to
add additional steps to the pipeline. Anytime you need to work with lists of tasks
or tasks of lists, or just a normal `IAsyncEnumerable`, AsyncLinq will make your life a lot 
easier.