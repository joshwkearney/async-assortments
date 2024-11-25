# Async Extensions for Enumerables (Axe.NET)

1. [Overview](#overview)
2. [Download](#download)
3. [Project goals](#project-goals)
4. [Why not use `System.Reactive.Linq` ?](#why-not-use-systemreactivelinq)
5. [What is an IAsyncEnumerable anyway?](#what-is-an-iasyncenumerable-anyway)
6. [Concurrent and parallel sequences](#concurrent-and-parallel-sequences)
7. [Async sequences and reactive programming](#async-sequences-and-reactive-programming)

## Overview
C# 8 added the `IAsyncEnumerable` interface to support async sequences, but .NET is still
missing the infrastructure to make it useful in many situations. This library aims to 
bridge the gap, providing support for `await`, LINQ, concurrent programming, and more to 
`IAsyncEnumerable`. Keep reading for details!

```csharp
var ids = new int[] { 1, 2, 3 /* etc */ };

// Create an IAsyncEnumerable from any IEnumerable
var seq = ids.ToAsyncEnumerable();

// Manipulate the sequence using LINQ (and async LINQ!)
var processedSeq = ids
    .ToAsyncEnumerable()
    .Where(x => x > 2)
    .AsyncSelect(async x => await ProcessItemAsync(x))
    .SelectMany(x => new[] { x, x });

// Asynchronously process each item as it comes in
await foreach (var item in processedSeq) {
    // ...
}

// Or directly await the whole IAsyncEnumerable to get a normal IEnumerable, just
// like awaiting a Task<T> gives you a T
var results = await processedSeq;

// Or collect the IAsyncEnumerable into a more specific type
var results2 = await processedSeq.ToDictionaryAsync(x => x, x => x);

// Create an async method with multiple return values using iterators
async IAsyncEnumerable<string> GetSequenceAsync() {
    yield return "Hello world!";
    await Task.Delay(1000);
    yield return "This is an async function that returns a sequence!";
}

// Process a list in parallel using async LINQ
// Normally you would use some ghastly combination of LINQ and Task.WhenAll for this
var results = await ids
    .ToAsyncEnumerable()
    .AsParallel(preserveOrder: false)
    .AsyncSelect(async x => await Step1Async(x))
    .Where(x => x.Something >= 100)
    .AsyncSelect(async x => await Step2Async(x))
    .Take(50);
```

## Download
The latest version of `AsyncCollections.Linq` can be downloaded on NuGet 
[here](https://www.nuget.org/packages/AsyncCollections.Linq/).

## Project goals
Microsoft has made it clear that they have 
[no intentions](https://github.com/dotnet/runtime/issues/31580#issuecomment-581065904) 
of shipping LINQ extensions for `IAsyncEnumerable` and they 
[don't think all useful functionality must be included in the standard 
library](https://github.com/dotnet/runtime/issues/31580#issuecomment-636364261). Well, 
challenge accepted! This project aims to provide high-quality infrastructure for
`IAsyncEnumerable`, including LINQ.

## Why not use `System.Reactive.Linq` ?
The incredible [Rx.NET project](https://github.com/dotnet/reactive) maintains the 
`System.Linq.Async` namespace, which contains LINQ operators for `IAsyncEnumerable`. Why
create a new project that aims to do the same thing?

Rx.NET assumes an `IAsyncEnumerable` represents a linear sequence of 
elements with a delay between each element. This is fine (but not ideal) for IO-bound
operations like a database query, but in C# tasks can also be used to represent 
*concurrent* operations running at the same time. What if we wanted to use 
`IAsyncEnumerable` to represent a collection of tasks that have already started?

The LINQ operators included in Rx.NET don't compose correctly under concurrent 
operations, so a new library was needed. See the section below for details.

## What is an `IAsyncEnumerable` anyway?
In C#, `IEnumerable` represents a lazy sequence, which means that the next element is not
computed until `MoveNext()` is called on the iterator. If you stop calling `MoveNext()`,
no more elements are produced. This means it is a pull-based sequence, because `IEnumerable`
requires the consumer to "pull" items out of it.

By contrast, a `Task` is eager, and it starts as soon as it's created. If you call a 
function that returns a task without awaiting, that function is running concurrently
in the background. When the task is done, it "pushes" the result to consumers 
by calling the task's callbacks. This means it is push-based, and runs
whether or not it is awaited by the consumer.

This puts `IAsyncEnumerable` in a very weird spot. Should it be lazy like an 
`IEnumerable`, or eager like a `Task`? 

The best answer is that `IAsyncEnumerable` should support both lazy and eager modes,
because it has to interact with both types of systems. By default,
an `IAsyncEnumerable` is lazy like a normal `IEnumerable`, and it only runs operators
when `MoveNextAsync()` is called on the iterator. Consider this code:

```csharp
// Takes 600 ms
var results = await new[] { 3, 2, 1 }
    .ToAsyncEnumerable()
    .AsyncSelect(async x => { await Task.Delay(x); return x; })
    .ToListAsync();
```

This does exactly what you would expect from a lazy sequence, because it calls
the selector as it iterates the items. In effect, this will do 
`MoveNext(), Delay(300), MoveNext(), Delay(200), MoveNext(), Delay(100)`, taking
600 ms in total. 

But what if we want this to be eager and behave more like a list of tasks? 
This is usually what you want `Task.WhenAll` for.

```csharp
// Takes 300 ms
var results = await new[] { 3, 2, 1 }
    .ToAsyncEnumerable()
    .AsConcurrent()
    .AsyncSelect(async x => { await Task.Delay(x); return x; })
    .ToListAsync();
```

By adding `.AsConcurrent()` we can tell the operators to process the elements at the 
same time once the sequence starts enumerating. This is now an eager sequence:
when we start to enumerate it with
`await .ToListAsync()`, all of the selectors from `AsyncSelect` will start at the 
same time instead of waiting for the previous ones to finish.

This works more like calling `await Task.WhenAll()` with a list of tasks. First 
we start all of the work, and then await the results afterwards. This will do
`Delay(300) + Delay(200) + Delay(100), MoveNext(), MoveNext(), MoveNext()`,
taking only 300 ms total.

## Concurrent and parallel sequences
We just saw how `AsConcurrent()` told later operators to behave eagerly like a list of
tasks, but there are actually a few different ways of doing this that each have their
own behavior. 

First there is `AsConcurrent()`, which runs async operators at the same time using 
tasks, much like you would when using `Task.WhenAll()`. However this still uses 
just one thread, and if you do
CPU-bound work the sequence will jam up and not run concurrently anymore.

For this situate there is also `AsParallel()`, which schedules async operators to run
on the thread pool instead. Think of this like `Task.Run`-ing all the async operators:

```csharp
// Takes 600 ms
var results = await new[] { 3, 2, 1 }
    .ToAsyncEnumerable()
    .AsConcurrent()
    .AsyncSelect(async x => { Thread.Sleep(x); return x; })
    .ToListAsync();

// Takes 300 ms
var results = await new[] { 3, 2, 1 }
    .ToAsyncEnumerable()
    .AsParallel()
    .AsyncSelect(async x => { Thread.Sleep(x); return x; })
    .ToListAsync();
```

Finally there is the matter of ordering. By default, `AsConcurrent()` and `AsParallel()`
preserve the order of the sequence when running their operators, which is usually what
you want. However, there are situations where you want finished operations to 
immediately continue down the pipeline without waiting on previous items. This will improve
the time until the first result, and it can massively improve performance if there is a lot
of time variability at each step.

```csharp
// Takes 100 ms
var results = await new[] { 3, 2, 1 }
    .ToAsyncEnumerable()
    .AsConcurrent(preserveOrder: false)
    .AsyncSelect(async x => { await Task.Delay(x); return x; })
    .FirstAsync();

// Returns 1, 2, 3
var results = await new[] { 3, 2, 1 }
    .ToAsyncEnumerable()
    .AsConcurrent(preserveOrder: false)
    .AsyncSelect(async x => { await Task.Delay(x); return x; })
    .ToListAsync();
```

## Async sequences and reactive programming
When an `IAsyncEnumerable` is operating in eager mode with `AsConcurrent()` 
or `AsParallel()`, it works more-or-less like an `IObservable` from
Rx.NET. If you squint, creating an async enumerator is basically the same as
subscribing to an observable.

That means it's easy to use async sequences with reactive programming, and you can
convert observables to async enumerables and back:

```csharp
var subject = new Subject<int>();

// This will subscribe to our subject and print 10 20 30 to the console
var task = Enumerate();

subject.OnNext(10);
subject.OnNext(20);
subject.OnNext(30);
subject.OnCompleted();

// This task will finish when our observable is complete
await task;

Console.WriteLine("Done!");

async Task Enumerate() {
    await foreach (var item in subject.ToAsyncEnumerable()) {
        Console.WriteLine(item);
    }
}
```
