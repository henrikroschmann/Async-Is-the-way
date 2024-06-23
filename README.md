# Async-Is-the-way-
It started out as a comparison between ConcurrentDictionary and Dictionary but turned into a history lesson 

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

```csharp
int largeAmountOfFiles = 100.000;
for (var i = 0; i < largeAmountOfFiles; i++)
{
    await ProcessDataAsync() // method from example above 
}
```





** There is so much more on this topic that I might add in the future and some parts I don't even know at this point. 
The goal is to give a useful and descriptive overview on history and give food for thought. **

* Example of when to skip async await to reduce ease the load of the state machine 
* Example calling a Async method in a synchrououse context

* Example of Task.WhenAny / Task.WhenAll ( provide example code ) Related to state machine 
* Example of concurrent Dictionary vs DIctionary ( provide example code)
* Example of concurrentBad vs list  ( provide example code )

* Example semaphoreSlim for awaiting 