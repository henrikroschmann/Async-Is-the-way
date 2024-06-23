# Async-Is-the-way-
It started out as a comparison between ConcurrentDictionary and Dictionary but turned into a history lesson 


# Table of Contents
1. [Async-Is-the-way-](#async-is-the-way-)
2. [Introduction](#introduction)
3. [Convert to Asynchronous](#convert-to-asynchronous)
4. [Block or Not to Block](#block-or-not-to-block)
5. [Async/await History](#asyncawait-history)
6. [Dispose?](#dispose)
7. [Return](#return)
8. [Skipping Gears](#skipping-gears)
9. [Awaits in Loops](#awaits-in-loops)
10. [Calling Async Method in Synchronous Context](#calling-async-method-in-synchronous-context)
11. [Other Known Issues with Async](#other-known-issues-with-async)
    - [Populating Dictionaries](#populating-dictionaries)
    - [Populating Lists](#populating-lists)
12. [Conclusion](#conclusion)

---


# Introduction
Async/ Await what a amazing piece of technology, the option to running processes in a non-blocking function.

Let's start from the top

```csharp
// Let's copy data from one source to another in a Synchronously manner
public void CopyStreamToStream(Stream source, Stream destination)
{   
    // Initialize a buffer with a size of 4096 byte
    // The value 0x1000 is in hexadecimal notation, which is equivalent to 4096 in decimal.
    var buffer = new byte[0x1000]; 
    // This buffer will be used to read and store chunks of data during streaming operations.
    // The code snippet is commonly used when handling streams (e.g., reading from a file or network stream).
    // It reads data in chunks (streaming) rather than loading the entire data into memory at once.

    int numRead;
    while ((numRead = source.Read(buffer, 0, buffer.Length)) != 0)
    {
        destination.Write(buffer, 0, numRead);
    }
}

```

##### *Explain the code:*
*The source.Read(buffer, 0, buffer.Length) reads a chunk of data from the source stream into the buffer. 
The numRead variable holds the actual number of bytes read (which may be less than the buffer size). 
The destination.Write(buffer, 0, numRead) writes that chunk to the destination stream. This process repeats until all data is transferred.*


> Fun fact
> -   *The size of the buffer (16 KB or 16 * 1024 bytes) is somewhat arbitrary.*
> -   *If the buffer were too small (e.g., one byte at a time), it would be slow.*
> -   *If it were too large (e.g., 1 GB), it would waste memory.*
> -   *16 KB strikes a balance between efficiency and memory usage.*

Let's convert this method into a asynchronous one 

```csharp
public async Task CopyStreamToStreamAsync(Stream source, Stream destination)
{       
    var buffer = new byte[0x1000]; 
    
    int numRead;
    while ((numRead = await source.ReadAsync(buffer, 0, buffer.Length)) != 0)
    {
        await destination.Write(buffer, 0, numRead);
    }
}
```

If you look a this code in a sloppy manner you would not even notice the difference, sure we added some extra keywords like **async** and **await** also **Task** instead of **void**. So what is the difference, what does it mean? 

## Block or not to block that is the question. 
The first example has a serial approach we execute the method and only that, we halt the freeze the application we block everything else. All eyes on it someone screaming "ME ME ME". 

The async approach is working in a non-blocking fashion, let's dig deeper into what happens. 

## Async/ await will rock your world

If we backtrack to .NET Framework 1.0 there was a Asynchronous Programming Model pattern called the APM pattern, also known as teh Begin/End pattern. Asynchronous has been a thorn in the side of developers for years, it has been described to be a useful way to avoid tying up a thread while waiting for some arbitrary task to complete, it has also been a pain to implement correctly, source: C# in depth, Jon skeet. 

> .Net Framework 1.x The BeginFoo / EndFoo approach using IAsyncResult and AsyncCallback to propagate result 
[Example: .NET Framework 1.x: Using BeginFoo / EndFoo with IAsyncResult and AsyncCallback](./Sample/AsyncWillRockYourWorld/Net1BeginEnd.cs)

.NET Framework 1.x: This version uses the BeginFoo/EndFoo pattern, which is quite low-level and involves manually managing asynchronous operations using IAsyncResult and AsyncCallback.

> The event-based asynchronous pattern from .Net 2.0, as implemented by BackgroundWorker and WebClient
[Example: .NET Framework 2.0: Using BackgroundWorker](./Sample/AsyncWillRockYourWorld/Net2BackgroundWorker.cs)

.NET Framework 2.0: Introduced the event-based asynchronous pattern with the BackgroundWorker class, which simplifies the process by using events to handle the background operation and its completion.

> (TPL) The Task Parallel Library introduced in .NET 4 and expanded in .NET 4.5
[Example: .NET Framework 4.0/4.5: Using Task Parallel Library (TPL)](./Sample/AsyncWillRockYourWorld/Net4TPL.cs)

.NET Framework 4.0/4.5: Introduced the Task Parallel Library (TPL), which provides a more powerful and flexible model for asynchronous programming using Task and async/await keywords.

> .NET Core / .NET 5: Using async/await with HttpClient
[Example: .Net 5/ Core HttpClient](./Sample/AsyncWillRockYourWorld/Net5.cs)

.NET Core / .NET 5: Uses HttpClient with async/await to perform asynchronous operations in a more modern, straightforward way.

### Dispose?

What about **DISPOSE**?, Stephen Toub, explains it well in the following [article](https://devblogs.microsoft.com/pfxteam/do-i-need-to-dispose-of-tasks/). 

>“Task implements IDisposable and exposes a Dispose method.  
>Does that mean I should dispose of all of my tasks?”
>
>In short - “No.  Don’t bother disposing of your tasks.” You don't need to dispose tasks in general. 
>There might be scenarios where Dispose can be utilized if you require full control, but not in general.

### Let's return 

In short Asynchronous operation will not halt all operation, example freeze the UI, while executing operations. 
When discussing threading in Windows Forms there are two golden rules 

* Don't perform any time-consuming action on the UI thread 
* Don't access any UI controls *other* than on the UI thread

.Net has embraced asynchronism wholeheartedly with the task-based asynchronous pattern to be consistent across multiple APIs. But it is not omniscient it cannot guess when to perform task synchronously and asynchronously. NET 5 removed most of the boilerplate code needed without the need for fluff we are now left with **async Task** and **await** 

Asynchronous functions, this is either a anonymous function or method they are declared using **async** modifier, and it can include **await** expressions. The **await** expression is where it gets interesting.

> **Anonymous function** using Task.Run
> ```csharp 
> await Task.Run(async () => await SomeOperation());
> ```
> **Method**
> ```csharp 
> await SomeOperation();
> ```
// create code examples

If the the expression what is being awaited isn't available yet, the async method will return immediately but when the value becomes available it will continue where it left off, in correct thread. 


### What about the compiler? How does it work? 

The compiler is creating a state machine. Let's take a simple example 

```csharp
Console.WriteLine("First we have winter");
Console.WriteLine("After winter we have summer");
```

In a synchronous way we expect the first call to complete *"First we have winter"* and then the second *"After winter we have summer"*. Execution flows from one to the other. But a asynchronous execution model does not work like that. It is all about *continuations*. WHen you start doing something you also say what you expect to happen next. My example above is somewhat silly but let's say that operation one requires more computation or latency if we have external API calls the one that returns first wins the race so we can end up with operation 2 returning before operation 1. 

When a task is awaited in a async context the async operation starts and returns a token that can be used to provide the continuation later on. This token represents a ongoing operation that might have completed or is still in progress. Typically the token is in the form of **Task** or **Task<TResult>**

> 1. Do some work
> 2. Start an asynchronous operation and remember the token it returns
> 3. Possibly do some more work. 
> 4. Wait for the asynchronous operation to complete, using the token 
> 5. Do some more work.
> 6. Finish

Source: *Flow in a asynchronous method .NET 5, C# in depth, Jon skeet*

Example: When you order a pizza you typically don't stand in the door and wait for the delivery dude/dudette to come (synchronous approach). You **await** after the call by returning to your standard task of watching tv until the token, pizza, arrives at the door and you take next action (Async). 

## let's dig 🪓 deeper State Machine


```csharp
await ProcessDataAsync();

static async Task ProcessDataAsync()
{
    Console.WriteLine("Starting data processing...");
    await Task.Delay(2000); // Simulate a delay
    Console.WriteLine("Data processed.");
}
``` 

Explanation
* await ProcessDataAsync(); calls an asynchronous method and waits for it to complete.

ProcessDataAsync Method:
* Marked as async, indicating it contains asynchronous operations.
* await Task.Delay(2000); is an asynchronous operation that simulates a delay of 2 seconds.

State machine 
* The state machine saves the current position and schedules the continuation of the method after the delay completes.
* The state machine allows the thread to do other work while waiting for the delay to complete.

Continuation:
* After the 2-second delay, the state machine resumes execution from where it left off.
* It moves to the next state, printing "Data processed." and eventually completes the method.


## So conclusion async all the way?

Not really, let's use the example from above and let's say we would like to execute **ProcessDataAsync** for a multitude of files 


### Skipping gears (reference to stick shift cars)

There are scenarios where uneccessary state machine utilizatino is done given the following example:


```csharp
await SomeTransaction();

public async Task SomeTransaction()
{
    await ExecuteTransaction();
}

public async Task ExecuteTransaction()
{
    await TheTransaction();
}
```

Using the metaphor of skipping gears: while driving a stick shift car, you aren't forced to use every gear sequentially—you can skip gears to accelerate more efficiently. In the same way, when awaiting asynchronous methods, you can skip intermediate methods to avoid additional overhead. By cutting out the middleman, you prevent unnecessary state machine allocations. See the revised example below:


```csharp
await SomeTransaction();

public Task SomeTransaction()
{
    // Skipping the state machine creation here
    ExecuteTransaction();
}

public async Task ExecuteTransaction()
{
    await TheTransaction();
}
``` 
// create code examples


#### Tell-tale Signs to Optimize async/await Usage:

* **Simple Forwarding Methods**: If your asynchronous method is only awaiting another task and returning its result, consider removing the async keyword and directly returning the task to avoid unnecessary overhead.
* **Performance Profiling**: If performance profiling indicates high overhead in simple async methods, removing unnecessary state machine overhead can be beneficial.
* **High-Frequency Calls**: For methods that are called frequently and involve simple asynchronous forwarding, optimizing for reduced overhead can improve overall application performance.


### What about awaits in loops? 

```csharp
var files = Directory.GetFiles(folderPath);
foreach (var file in files)
{
    await ProcessDataAsync(file);
}
```
If the goal is to chain the requests await the result of the previous execution this is a good approach but when you use await inside a loop like this, each iteration waits for the previous one to complete before starting the next. This sequential execution can be inefficient, especially if the operations are independent and can be executed concurrently. A better solution is to set up a task list and await them concurrently.

```csharp
var files = Directory.GetFiles(folderPath);
// create task list 
var tasks = files.Select(file => await ProcessDataAsync(file));
// Parallel Processing
Task.WhenAll(tasks);
``` 
// create code examples

This is much more efficient for I/O-bound tasks, as it can process multiple files concurrently. It takes full advantage of the asynchronous programming model, potentially reducing overall processing time significantly.

#### Tell-tale Signs to Use Task.WhenAll:

* **Independent Tasks**: When you have multiple asynchronous tasks that can run independently and do not depend on each other, using Task.WhenAll can significantly improve performance.
* **Sequential Loop Execution**: If you're currently using await inside a loop, leading to sequential execution, refactoring to use Task.WhenAll can make the operations concurrent and more efficient.
* **Performance Bottlenecks**: If performance profiling indicates that sequential execution of tasks is a bottleneck, consider using Task.WhenAll to execute them concurrently.
* **Handling Multiple Requests**: When dealing with a batch of requests (e.g., sending multiple mediator requests), using Task.WhenAll allows you to manage and await all requests efficiently.



### Calling a Async method in a synchronous context

Calling an asynchronous method in a synchronous context is possible. It is done using GetAwaiter().GetResult(), but it should be done with caution due to potential issues such as deadlocks. 

```csharp
var result = TaskAsync.GetAwaiter().GetResult();

public async Task<string> TaskAsync()
{
    await Task.Delay(1000);
    return "Complete";
}
```
[Example in code](./Sample/AsyncSamples/AsyncInSynchronous.cs)


Using GetAwaiter().GetResult() can cause deadlocks in certain synchronization contexts, particularly in GUI applications or ASP.NET environments where the synchronization context captures the current thread.

This approach blocks the calling thread, negating the benefits of asynchronous programming. It should be used sparingly and only when absolutely necessary.

If you need to call an asynchronous method from a synchronous context, consider the following. 

* Avoid blocking calls 
* ConfigureAwait, use ConfigureAwait(false) in your asynchronous methods to avoid capturing the synchronization context, which can help prevent deadlocks.

Updated example:
```csharp
var result = TaskAsync.GetAwaiter().GetResult();

public async Task<string> TaskAsync()
{
    await Task.Delay(1000).ConfigureAwait(false);
    return "Complete";
}
```
[Example in code](./Sample/AsyncSamples/AsyncInSynchronous2.cs)

### Other known issues with it comes to Async 

#### Populating dictionaries

Will not go into depth about dictionaries here, it's a container that maintenance a key-value pair. 

```csharp
private static Dictionary<int, string> dict = new Dictionary<int, string>();
var tasks = new List<Task>();
for (int i = 0; i < 10; i++)
{
    int localI = i;
    tasks.Add(Task.Run(() => AddItem(localI)));
}

Task.WhenAll(tasks).Wait();

private static void AddItem(int i)
    {
        lock (dict)
        {
            dict[i] = $"Value {i}";
        }
    }
```
[Example in code](./Sample/AsyncSamples/DictionaryInAsyncLoop.cs)

In this scenario we don't know when the key is added to the dictionary due to Task.WhenAll execute the operations in parallel. We can end up with key duplication in the dictionary. This can be prevented with the introduction of locks or utilize libs like semaphoreSlim or... the built in **ConcurrentDictionary**

```csharp
 private static ConcurrentDictionary<int, string> dict = new ConcurrentDictionary<int, string>();
var tasks = new List<Task>();
for (int i = 0; i < 10; i++)
{
    int localI = i;
    tasks.Add(Task.Run(() => AddItem(localI)));
}

Task.WhenAll(tasks).Wait();

private static void AddItem(int i)
    {
        lock (dict)
        {
            dict[i] = $"Value {i}";
        }
    }
```
[Example in code](./Sample/AsyncSamples/ConcurrentDictionaryInAsyncLoop.cs)

Locks where just mentioned and this is what is use in the concurrentDictionary. What does lock do? Even if the execution is async and running in parallel the lock will serialize the approach. 


#### Populating Lists

List is a container that stores item of specified type. Like with the previous example 

```csharp
private static List<int> list = new List<int>();
var tasks = new List<Task>();

for (int i = 0; i < 10; i++)
{
    int localI = i;
    tasks.Add(Task.Run(() => AddItem(localI)));
}

Task.WhenAll(tasks).Wait()

private static void AddItem(int i)
{
    lock (list)
    {
        list.Add(i);
    }
}
```
[Example in code](./Sample/AsyncSamples/ListExample.cs)


A ConcurrentBag is designed for concurrent access from multiple threads, whereas a List is not thread-safe and should not be accessed concurrently without external synchronization. Here are examples demonstrating the differences:

```csharp
private static ConcurrentBag<int> bag = new ConcurrentBag<int>();
var tasks = new List<Task>();

for (int i = 0; i < 10; i++)
{
    int localI = i;
    tasks.Add(Task.Run(() => AddItem(localI)));
}

Task.WhenAll(tasks).Wait()

private static void AddItem(int i)
{
    lock (list)
    {
        list.Add(i);
    }
}
```
[Example in code](./Sample/AsyncSamples/ConcurrentBagExample.cs)


Conclusion: Use ConcurrentDictionary and ConcurrentBag for thread-safe operations without needing explicit locks.

Conclusion: Use Dictionary and List when thread safety is not a concern or when you can ensure proper synchronization.


**There is so much more to this topic that I might add in the future and some parts I don't even know at this point. 
The goal is to give a useful and descriptive overview on history and give food for thought.**



